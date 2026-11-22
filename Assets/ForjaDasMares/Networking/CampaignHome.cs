using System;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    // Who is playing: the account and its single character (AccountService, made in the "Inicio" scene).
    // The old local-profile keys remain only as a fallback for tests.
    public static class CampaignHome
    {
        public static string LocalAccountId=>AccountService.HasCharacter?AccountService.Peek().accountId:PlayerPrefs.GetString("Forja.LocalAccount.Id","");
        public static string LocalAccountFolder=>AccountService.HasCharacter?AccountService.CharacterFolder
            :Guid.TryParseExact(PlayerPrefs.GetString("Forja.LocalAccount.Id",""),"N",out _)
            ?System.IO.Path.Combine(Application.persistentDataPath,"RpgAccounts",PlayerPrefs.GetString("Forja.LocalAccount.Id","")):null;
        static string CharacterKey
        {
            get
            {
                if(LocalAccountFolder!=null)return "Forja.RpgCharacter."+LocalAccountId;
                var args=Environment.GetCommandLineArgs();
                int i=Array.IndexOf(args,"-netIdentity");
                return i>=0&&i+1<args.Length&&Guid.TryParse(args[i+1],out var id)
                    ?"Forja.RpgCharacter."+id.ToString("N"):"Forja.RpgCharacter";
            }
        }
        public static string CharacterName=>AccountService.HasCharacter?AccountService.Character.name:PlayerPrefs.GetString(CharacterKey+".Name","");
        public static int CharacterClass=>AccountService.HasCharacter?AccountService.Character.look:PlayerPrefs.GetInt(CharacterKey+".Class",2);
    }
}
