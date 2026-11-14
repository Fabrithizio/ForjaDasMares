# Cidade viva — 0.29.0

## Entrega
- 15 modelos originais no Blender, 30 FBX LOD0/LOD1, 15 prefabs Unity e paleta compartilhada. Fonte, catálogo, previews e validação: ArtSource/CityLifeKit.
- Integração na cidade: portão sul modelado, fachada temática no pátio de expedições, protetor central, seis barracas com mercadorias diferentes, pavilhão e quadro de avisos, bandeirolas e decoração de jardim/tecnologia antiga.
- Três reservas autorais com IDs e dimensões: eventos no oeste, futuros moradores ao sul da praça e expansão a leste. TownActivitySpace desenha limites somente quando selecionado no Editor; não cria paredes invisíveis nem botões falsos. A expansão fica atrás da muralha e ainda não é uma passagem funcional. O pavilhão é o espaço físico, não um evento jogável já criado.
- Seta discreta no chão da saída, visível por proximidade. Clicar aproxima o personagem pelo caminho existente e usa a transição real; atravessar o limite com movimento também funciona. Não é teletransporte instantâneo no ponto em que o mouse clicou. Indicador não é um FBX adicional: é uma malha Unity pequena.
- Preservados os NPCs provisórios, fonte/cura, instrutora, chegada, treino, missão legada e saves. Téo foi deslocado para a frente da nova banca; diálogo permanece informativo, sem loja falsa.

## Desempenho
Cada barraca tem entre 1.328 e 1.852 triângulos no LOD0 e cerca de 55% disso no LOD1; uma submalha/material por nível. Sem tecido simulado, luzes extras, sombras de transparência ou scripts por banca. Colliders de balcão/postes separados da cobertura.
Auditoria de 67 prefabs de cenário: Docs/Validation/0.29.0/asset-audit.json. Maiores malhas de cenário existentes nessa amostra: cerca de 11–13 mil triângulos, já com LOD. Isso isoladamente não prova gargalo: quantidade visível, sombras, materiais, texturas e overdraw também importam. Não foram apagadas ou decimadas cegamente.
80 instâncias de árvores/arbustos da cidade receberam limiar de LOD ajustado. Árvores do kit anterior passam ao LOD1 antes: aproximadamente 10 mil para 1,7 mil triângulos por árvore quando o nível simplificado está ativo. Não é redução garantida de 83% no custo total do jogo nem promessa de FPS. A transição precisa de avaliação artística, sobretudo em telas grandes.

## Arquivos principais
- ArtSource/CityLifeKit/*: fonte Blender, gerador, catálogo, renderizações e validação.
- Art/World/CityLifeKit/*: modelos e prefabs usados pelo jogo.
- Networking/Editor/CityLifeInstaller.cs: importação, integração, reservas, navegação e auditoria.
- Networking/World/TownActivitySpace.cs: reservas editoriais para expansão.
- Networking/World/TownExitCue.cs: visibilidade do indicador por distância.
- Networking/Tests/CityLifeTests.cs: variantes, orçamento de geometria, vãos, reservas, LOD e indicador.
- Scenes/RefugioUltimoSino.unity e Data/World/Town/Navigation.asset.
- Networking/Editor/CampaignInstaller.cs: versão 0.29.0, código Android futuro 52.

## Conferir no PC
Abrir Builds/Atual/Windows/ForjaDasMaresCampanha.exe e entrar na cidade. Visitar mercado, praça, pátio de expedições e espaço de eventos. Barracas decorativas não mostram compra. Caminhar/clique na seta da saída carrega a costa; placa de expedições abre o lobby existente. Aproximar/afastar da seta para verificar sua visibilidade. Conferir F8 e zoom em regiões com árvores; observar possíveis transições de detalhe.
Na Unity, selecionar os objetos Reserva_* sob TownEnvironment/CityLife029 para ver as áreas planejadas. Novos NPCs/eventos deverão usar essas áreas sem bloquear circulação; não foram implementados neste bloco.

## Limites
Não houve APK novo, teste físico Android, teste online, nova missão, nova economia, inimigo ou animação. A arquitetura visual e os recursos usados consideram celular, mas desempenho no aparelho continua pendente. Arte é proposta estilizada, não reprodução final da imagem conceitual.
Resultados finais de validação/build e capturas serão registrados em Docs/Validation/0.29.0/summary.json. Rodadas repetidas não representam testes únicos adicionais.

Validação final: 30/30 FBX reimportados; 23/23 testes Unity passaram; build Windows errors=0. Capturas reais de portão/mercado no relatório. Permanecem materiais estilizados simples, ajuste fino de disposição e avaliação das transições de LOD; a câmera de jogo não mostra toda a cidade de uma vez.
