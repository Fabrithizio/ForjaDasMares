using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ForjaDasMares.Networking
{
    // A separate, versioned local story journal. It never writes online inventory or currency.
    [DefaultExecutionOrder(240)]public sealed class RegionalJourney:MonoBehaviour
    {
        [Serializable]public sealed class Journal
        {public int version=1;public string questId;public bool accepted,completed;public int clues;}
        [SerializeField]RegionalWorld world;
        [SerializeField]RegionStoryDefinition definition;
        Journal journal;string path;bool travelling;
        static string testRoot;
        public string Notice {get;private set;}
        public RegionStoryDefinition Definition=>definition;
        public bool Accepted=>journal!=null&&journal.accepted;
        public bool Completed=>journal!=null&&journal.completed;
        public int MemoryFragments=>Completed?1:0;
        public int ClueCount {get{int bits=journal?.clues??0;return (bits&1)+((bits>>1)&1)+((bits>>2)&1);}}
        public string Objective=>!Accepted?"Fale com Nara no refúgio.":Completed?"Sino restaurado · Fragmento de Memória recebido.\nExplore ou entre nas catacumbas ao norte.":ClueCount>=definition.requiredClues?"Leve as pistas para Daro, no refúgio.":"Investigue cais, pedreira ou jardim: "+ClueCount+" / "+definition.requiredClues+" pistas. Escolha a ordem.";
        public void Configure(RegionalWorld region,RegionStoryDefinition story){world=region;definition=story;world.SetJourney(this);}
        void Start()
        {
            if(testRoot==null)testRoot=Path.Combine(Application.temporaryCachePath,"Act1Tests",Guid.NewGuid().ToString("N"));
            string root=Path.Combine(Application.persistentDataPath,"Act1LocalStory");var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length;i++){if(args[i]=="-runTests")root=testRoot;if(args[i]=="-netSavePath"&&i+1<args.Length)root=Path.Combine(Path.GetFullPath(args[++i]),"Act1LocalStory");}
            path=Path.Combine(root,"journal-v1.json");journal=ReadJournal(path,definition.questId,definition.requiredClues)??ReadJournal(path+".bak",definition.questId,definition.requiredClues)??new Journal{questId=definition.questId};
            Notice=Completed?"Bem-vindo de volta. O sino continua restaurado.":"Nara espera você junto à tenda do refúgio.";
        }
        public static Journal ReadJournal(string filename,string quest,int requiredClues=2)
        {
            try
            {
                if(!File.Exists(filename))return null;var data=JsonUtility.FromJson<Journal>(File.ReadAllText(filename));
                if(data==null||data.version!=1||data.questId!=quest||data.clues<0||data.clues>7||data.completed&&(!data.accepted||((data.clues&1)+((data.clues>>1)&1)+((data.clues>>2)&1))<requiredClues))return null;
                return data;
            }catch(IOException){return null;}catch(ArgumentException){return null;}catch(UnauthorizedAccessException){return null;}
        }
        bool Commit(Journal next)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(next,true));
                if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
                journal=next;return true;
            }catch(IOException){Notice="Não foi possível salvar. Tente interagir novamente.";return false;}
            catch(UnauthorizedAccessException){Notice="Sem acesso ao salvamento local. Tente novamente.";return false;}
        }
        public int NearestPoint()
        {
            if(journal==null)return -1;float best=definition.interactionDistance*definition.interactionDistance;int result=-1;
            for(int i=0;i<definition.points.Length;i++){var delta=definition.points[i].position-world.Hero.transform.position;delta.y=0;if(delta.sqrMagnitude<best){best=delta.sqrMagnitude;result=i;}}
            return result;
        }
        public string Hint {get{int i=NearestPoint();return i>=0?"F · "+definition.points[i].label:"";}}
        public bool Interact()
        {
            int index=NearestPoint();if(index<0||travelling||!world.Hero.Health.IsAlive)return false;
            var point=definition.points[index];var next=JsonUtility.FromJson<Journal>(JsonUtility.ToJson(journal));
            switch(point.interaction)
            {
                case RegionalInteraction.Cartographer:
                    if(!Accepted){next.accepted=true;if(Commit(next))Notice="Nara: a expedição passou pelo cais, pedreira e jardim. Procure pistas em dois desses lugares e leve-as a Daro.";}
                    else Notice=Completed?"Nara: o sino voltou. Agora podemos investigar a Âncora nas catacumbas.":"Nara: siga os caminhos sinalizados. Você escolhe por onde começar.";
                    break;
                case RegionalInteraction.Clue:
                    if(!Accepted){Notice="Converse com Nara no refúgio para entender estes rastros.";break;}
                    if((journal.clues&(1<<point.clue))!=0){Notice="Esta pista já está registrada no diário.";break;}
                    next.clues|=1<<point.clue;if(Commit(next))Notice=point.discovery+(ClueCount>=definition.requiredClues?" Volte a Daro no refúgio.":"");
                    break;
                case RegionalInteraction.Artificer:
                    if(Completed){Notice="Daro: o Fragmento de Memória já é seu. A entrada das catacumbas está liberada.";break;}
                    if(!Accepted||ClueCount<definition.requiredClues){Notice="Daro: preciso de pistas de pelo menos dois locais para restaurar o sino.";break;}
                    next.completed=true;if(Commit(next)){Notice="Daro restaurou o sino. Recompensa: 1 Fragmento de Memória. Acesso às catacumbas liberado.";world.Hero.Health.Restore();}
                    break;
                case RegionalInteraction.Catacombs:
                    if(!Completed){Notice="A entrada está selada. Ajude Nara e Daro a restaurar o sino.";break;}
                    travelling=true;world.Hero.GetComponent<ArenaInput>().BlockCommands(true);SceneManager.LoadSceneAsync("CatacumbasDasMares");break;
            }
            return true;
        }
    }
}
