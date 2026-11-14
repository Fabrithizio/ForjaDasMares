# Ambiente 0.27.0 — em produção, 05/11/2026

**Registro histórico do bloqueio inicial, já resolvido.** O titular concluiu a ativação; a integração, os testes e os builds passaram depois. Consulte `AMBIENTE_0_27_0.md` para o estado atual, instalação no moto g15 e limitações. O texto abaixo preserva onde a execução havia parado.

## Ordem retomada no Trello

A01 cidade e primeiro trecho aberto; A02 integração do kit como dependência; A03 roteiro ainda a detalhar; A04 resposta visual por interação; A05 primeira aventura e A06 segredos após validar a base. Não marcar o mundo completo como entregue. Quadro: https://trello.com/b/DyOfIMBp/jogo-das-mares

## Trabalho preparado

- Kit original com 36 peças, 72 FBX e paleta copiados para `Assets/ForjaDasMares/Art/World/SettlementKit`. Reimportação prévia no Blender: 72/72, sem faces de área zero após limpeza. Isso não comprova importação, iluminação ou desempenho na Unity.
- `TownArtInstaller`: importação URP, prefab com LOD0/LOD1 por peça, migração pontual da cidade e decoração de duas rotas já existentes na costa. Não chama instaladores que recriam o projeto/campanha.
- Planeja substituir três casas, arbustos esféricos, bancos, mercado e fonte; adicionar árvores, vegetação, sinalização e objetos com função por área. NPCs provisórios e treino permanecem funcionais na cena original. Não implementa interiores das casas.
- Colisões simples distintas da copa; sem scripts por folha/planta, sem luz dinâmica por poste. Navegação deve ser recalculada e destinos validados antes de substituir seus dados estáveis.
- `WorldInteractionFeedback`: efeito de apresentação acionado pela interação existente da fonte, sem conceder recompensas nem duplicar a cura. Captura/restaura transformações e property blocks, remove listener ao desabilitar. É o primeiro caso de feedback, não cumprimento universal da regra A04.
- `TownArtTests`: checks de LOD, materiais, serviços alcançáveis e cura/feedback sem listeners repetidos. Ajuste do teste legado da costa para admitir dois ou três LODs conforme o kit.

## Estado real da verificação

`Logs/settlement027-install.log`: Unity 6000.3.26f1 terminou com código 198, “No valid Unity Editor license found”. A função Apply NÃO rodou. Nenhuma cena/prefab/navmesh foi gerada por esta migração; não houve PlayMode, build Windows, medição de FPS ou teste visual em jogo nesta etapa. Os testes novos estão escritos, não executados.

## Retomar após resolver a licença

1. Titular lê os termos no Hub, aceita se concordar, entra na conta e ativa a licença apropriada. Não alterar arquivos de licença nem contornar a ativação.
2. Com o Editor fechado para a execução por lote, executar `ForjaDasMares.Networking.Editor.TownArtInstaller.Apply`, ou usar o menu `Forja > World > Integrar kit da cidade (preserva missao)`.
3. Inspecionar logs, corrigir erros de compilação/importação, conferir escala e material na Unity. Somente então considerar os prefabs/cenas realmente integrados.
4. Rodar TownArtTests, TownTests, ActOneTests, ArpgControlsTests e InventoryClarityTests. Corrigir os problemas antes de prosseguir.
5. Gerar Windows por `CampaignInstaller.Windows`, em `Builds/Atual/Windows`, mantendo um único executável estável. A versão 0.27.0 só deve ser entregue após validação.
6. Testar cidade, conversa, fonte, treino, passagem para a costa, duas rotas, sombras/oclusão e F8. Não afirmar 30 FPS Android sem medição física; não gerar APK sem pedido.

## Pendências de conteúdo

Roteiro de origem do jogador e conflito inicial continuam abertos; não definir causas da ruptura por conta da importação de um asset. O vestígio antigo existente, a missão do sino, o diário e os IDs não foram reescritos. A cidade ainda é prévia local, sem região pública de 20 jogadores. Persistência de novas ações, interação de cada prop e sincronização online não surgem automaticamente com a malha.

## Arquivos de código desta etapa

Novos: `Networking/Editor/TownArtInstaller.cs`, `Networking/World/WorldInteractionFeedback.cs`, `Networking/Tests/TownArtTests.cs`.
Alterados: `Networking/Tests/ActOneTests.cs`, `Networking/Editor/CampaignInstaller.cs` (versão futura de build).
Documentação: este relatório e `PROGRESSO.md`. Sem pacotes novos.
