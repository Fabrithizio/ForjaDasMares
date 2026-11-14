# Inventário no RPG — 0.31.0 (06/11/2026)

O RPG reutiliza a interface funcional da campanha/desafios. Abra pelo botão INVENTÁRIO no canto inferior esquerdo ou I; feche por FECHAR ou Esc. Personagem, mochila, status, talentos, comparação com equipamento do mesmo slot, equipar/desequipar, reciclagem e entregas usam os sistemas existentes.

## Integração
- Refúgio, CostaAto1, CostaDasRuinas e Catacumbas recebem RegionalInventory, sem reconstruir mapas.
- Mesmo catálogo Forge/Catalog.asset e perfil local por participante em CampaignProfiles-v1. Respeita -netSavePath/-netIdentity; não importa cache de outro host.
- HeroEquipment aplica equipamentos, atributos e arma ao herói real. Trocar de cena não concede itens iniciais novamente.
- Interface bloqueia movimento/combate enquanto aberta. Gerenciamento exige personagem vivo, fora de perigo/ações e sem expedição pendente. Não concede recompensas novas no RPG.
- Adaptadores preservam o caminho de validação da campanha online. Não cria economia online regional ou servidor persistente.
- Corrigido acesso nulo à malha no WeaponSweepRibbon ao vincular visual ainda sem Mesh.

## Arquivos
Novos: InventoryContext.cs; CoopInventoryHud.Regional.cs; World/RegionalInventory.cs; Editor/RegionalInventoryInstaller.cs; Tests/RegionalInventoryTests.cs (e metas).
Alterados: CoopInventoryHud.cs, CoopInventoryHud.Areas.cs, CampaignCharacterSheet.cs, World/RegionalWorldHud.cs, Editor/CampaignInstaller.cs; quatro cenas acima; Scripts/Presentation/WeaponSweepRibbon.cs; ProjectSettings/ProjectSettings.asset (versão).
Caminhos de scripts relativos a Assets/ForjaDasMares/Networking, exceto onde indicado.

## Validação
38/38 testes PlayMode, zero falhas: RegionalInventoryTests, InventoryClarityTests, TownTests, TownArtTests, TownLayoutTests. Incluem ações reais da UI, atributos/equipagem, leitura do salvamento após equipar, viagem entre cenas sem duplicação, catálogo compartilhado, morte e entrada sintética I/Esc via Input System. Dados de testes isolados dos saves do jogador.
Build Windows: Succeeded, errors=0, Logs/rpg-inventory-windows.log. Executável estável: Builds/Atual/Windows/ForjaDasMaresCampanha.exe.
Conferência visual no player: mochila real do perfil local (30/36, 615 ouro), aberta e fechada por botões. Teclas nativas sintetizadas pela ferramenta não produziram resposta confiável; atalhos aprovados no teste Input System, ainda recomendada conferência manual física.

## Como testar
1. Abra a cidade no PC e clique INVENTÁRIO (ou I).
2. Selecione um item; veja comparação com o slot equipado, equipe e confira status/personagem.
3. Desequipe; confirme retorno à mochila, respeitando capacidade/entregas existentes.
4. Troque de mapa/reabra o jogo; confira preservação do equipamento e itens.
5. Feche a mochila e confirme retorno do movimento.

## Limitações
Sem teste Android físico, multiplayer real ou teste prolongado nesta etapa. Sem novo APK: o último continua 0.30.0/code53, anterior a esta integração. O envio desse APK pelo WhatsApp aguarda login do titular. Nenhuma arte, mapa ou sistema de loot adicional foi criado.