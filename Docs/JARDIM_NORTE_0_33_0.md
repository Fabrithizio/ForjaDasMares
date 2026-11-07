# Refúgio: Jardim Norte — 0.33.0

- Área de chão navegável ampliada de 76 × 76 para 76 × 104 unidades. Duas alamedas contornam o edifício norte e ligam o refúgio ao jardim. A fonte saiu do lado do braseiro e virou o ponto central do jardim; a fonte interativa existente permanece funcional.
- Entrada de estação e decoração de máquina desativadas na cena; acesso aos desafios preservado pelo quadro. Assets originais preservados para recuperação.
- Grama em tom menos saturado, importada com limite de 256 px e sem filtragem anisotrópica extra. 80 renderizadores de vegetação pequena deixam de projetar sombras. Novas peças reutilizam prefabs com LOD; piso decorativo não adiciona colisores.
- Arquivos: Networking/Editor/TownGardenExpansion.cs, CampaignInstaller.cs, Scenes/RefugioUltimoSino.unity, Data/World/Town/{Region,Navigation}.asset, Surroundings com materiais e malha do terreno, importador de GrassFantasy032.
- Validação: instalador verificou caminho completo refúgio → jardim. Rodada final 29/29 PlayMode, build Windows sem erros. Sem comparação controlada de FPS, teste Android ou APK. Para testar: entrar no RPG, seguir por um lado da construção ao norte até a fonte e pressionar F8.
- Referência artística: praça com foco, ruas curvas e largas, arquitetura arredondada, pedra/madeira, flores em canteiros e paleta coerente. Esta versão é expansão funcional com assets existentes, não a reconstrução da referência.
- Quaternius Stylized Nature MegaKit Standard: CC0 e uso comercial declarado pelo autor, https://quaternius.com/packs/stylizednaturemegakit. Não importado nesta versão.
- A coleção https://itch.io/c/5566841/placeboeffects-collection lista principalmente packs PSX de cenário e não oferece um pacote único de VFX com licença comum. Alternativa de sprites CC0: https://kenney.nl/assets/particle-pack. Nenhum efeito importado nesta versão.
