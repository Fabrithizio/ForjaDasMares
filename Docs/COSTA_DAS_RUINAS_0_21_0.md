# Costa das Ruínas — primeira região piloto

Escopo: cenário novo de exploração local, separado dos desafios atuais. Ainda não é região pública, servidor dedicado ou sessão de 20 jogadores. Não altera o save/inventário/economia do usuário. Reutiliza heróis, Input System, PlayerMotor, PlayerAim, ArenaCamera, HeroCombat, EnemyPool, IA e efeitos reais existentes.

## Conteúdo
Cena CostaDasRuinas.unity. Quatro setores Terrain de 80x80m (160x160m de área), relevo suave, caminhos pintados, costa/água, limites físicos e névoa. Refúgio em (0,-50), três acampamentos (Vigia do Cais, Pátio dos Esquecidos, Santuário Partido), rota principal, trilha secundária sinalizada por pedras e portal para os desafios. Os acampamentos usam três arquétipos existentes (saqueador, arqueiro, guardião), com 3/3/4 vagas e limite global 12; modelos de inimigos atuais preservados, sem fingir conversão das imagens em modelos animados.

Missão local Ecos na Costa: vencer a primeira patrulha de cada acampamento abre o portal. Cada vaga inicial só conta uma vez; respawns posteriores não duplicam conclusão. Respawn de 45s, retido enquanto o jogador estiver a menos de 8m; ativação até 75m. IA existente, com linha de visão e navegação. Reaparecimento/descanso no refúgio. Troca de herói apenas perto do refúgio. Portal retorna à campanha/desafios, não inicia automaticamente uma nova partida ou concede recompensa.

HUD próprio com vida, objetivo, minimapa de pontos de interesse, ação contextual, morte/reaparecimento e retorno ao menu. Música/ambiente usam clipes e preferências existentes, sem múltiplas músicas sobrepostas. Não há interface de inventário regional nem transferência de equipamentos/progresso para o mundo nesta fatia; essas integrações aguardam a autoridade/persistência regional. A missão é temporária por visita e não oferece itens/ouro falsos.

## Assets
Sete modelos de cenário fornecidos via ZIP preparados com pivô no chão e footprint padronizado de oito metros. Três LODs por modelo: original (~11–13 mil triângulos), ~50% e ~22%. Contagens reais em ArtSource/RegionalWorld/lod_report.json. Materiais URP com base/normal/metallic-smoothness; texturas 1K/mipmaps/compressão. Colisões simplificadas, separadas do detalhe decorativo. LOD e colisões são medidas técnicas; aparência ainda requer avaliação humana.

Os originais externos permanecem intocados. Assets usados pelo jogo e geradores são versionados; ZIPs/texturas originais 2K permanecem externos e precisam de arquivo próprio. Veja ArtSource/RegionalWorld/README.md para reprodução. Texturas de solo/grama/pedra são ruído original simples; água, sinalização e limites ainda são placeholders funcionais, não arte final.

## Entrada e controles
No menu inicial: EXPLORAR MUNDO · PROTÓTIPO LOCAL. WASD/setas, mouse para apontar/segurar ataque, E habilidade, Espaço esquiva, Q super, F interação, Tab herói no refúgio, R retorno ao refúgio. A tela de morte também tem botão Reaparecer. VOLTAR AO MENU preserva o acesso à campanha antiga.
Na Unity, abrir Assets/ForjaDasMares/Scenes/CostaDasRuinas.unity e Play. A cena está na lista compartilhada de build. O terreno e a navegação ficam em Data/World. Reinstalação explícita por RegionalWorldInstaller.Apply recria só a variante regional a partir da campanha; não substituir a cena da campanha.

## Verificações e limites
Cinco RegionalWorldTests passaram: quatro terrenos e caminhos NavMesh completos para todos os destinos, spawn/limite/respawn sem duplicar conclusão, morte/reaparecimento seguro, LODs com renderers/materiais válidos e movimento por entrada real. Primeiro ensaio detectou falta da cena na lista de build, corrigida por RegisterScene; minimapa usa Graphic próprio em objeto filho para respeitar a limitação de um Graphic por objeto.
Build e conferência no player: registros em Logs/region-windows.log e Logs/RegionQA. Desempenho de Android, sessões online, 20 jogadores, carga de mil usuários, perfil permanente regional e travessia entre servidores não testados/implementados. Sem APK novo, servidor contratado ou serviços cobrados nesta etapa. As metas 60fps PC/30fps Android seguem metas, não resultados medidos.

## Arquivos
Novos scripts: Networking/World/RegionDefinition.cs, RegionalWorld.cs, RegionalWorldHud.cs, RegionalProbe.cs (QA opt-in em builds de desenvolvimento), Networking/Editor/RegionalWorldInstaller.cs e Tests/RegionalWorldTests.cs. Cena CostaDasRuinas.unity; Data/World (quatro TerrainData, terreno/material/layers, navmesh e definição); Art/World/Coast (sete modelos/prefabs/LODs e texturas); ArtSource/RegionalWorld (geradores/relatório).
Alterados: CampaignHome.cs (entrada), Editor/CampaignInstaller.cs (cena incluída e versão), asmdef do editor (Navigation/Input System), Packages/manifest.json e lock (módulos Unity Terrain/TerrainPhysics), ProjectSettings/EditorBuildSettings.asset, ProjectSettings/ProjectSettings.asset, .gitignore e Docs/PROGRESSO.md.

Próxima etapa técnica: separar party de instância pública e validar servidor dedicado local com poucos clientes; ampliar para 20 só após testes. Primeira região ainda precisa de revisão de composição visual, história/NPCs, balanceamento e experiência de exploração com o usuário.
