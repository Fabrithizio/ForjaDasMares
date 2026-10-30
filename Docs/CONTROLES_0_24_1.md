# Controles e atualização Android — 0.24.1

30/10/2026. Escopo: corrigir ataque/deslocamento indesejado no PC e atualizar a mesma instalação no moto g15. Não adiciona regiões públicas, missões ou equipamentos novos.

## Regras do PC

- Mouse RPG: clicar/segurar no chão anda; não dispara.
- Nos dois modos F8, clique/segure sobre inimigo ou objeto que recebe dano para atacar. O personagem para imediatamente, inclusive se estava andando por clique ou WASD. Não há aproximação automática.
- Fora do alcance ou com obstáculo: permanece parado e informa o motivo. Solte o ataque e aproxime-se manualmente.
- Segure Shift e ataque para disparar parado na direção do cursor, sem precisar de alvo. Alterar cursor atualiza os ataques seguintes; projéteis já lançados continuam retos.
- Sair com o cursor do alvo mantendo o mesmo botão pressionado não transforma esse gesto em caminhada. Novo deslocamento por clique exige soltar e clicar novamente no chão.
- Movimento continua normal quando não está atacando; esquiva, investida e knockback não são apagados por StopWalking.
- NPCs, baús de interação e decoração sem vida não viram alvos de dano. Objetos quebráveis da campanha com Health continuam aceitos pelo sistema existente.
- Ataques já iniciados mantêm sua resolução/cadência, carga e recuperação; esta revisão não redesenha as habilidades.

Referência consultada: instruções oficiais de Drakensang Online, [Basics](https://board-en.drakensang.com/threads/basics.60450/) e [Combat](https://board-en.drakensang.com/threads/combat.60412/). Elas descrevem alvo por mouse e Shift estacionário. A regra adicional de não perseguir foi solicitada pelo usuário e implementada no código próprio do projeto.

## Implementação

DesktopRpgControls mantém gesto de ataque separado da navegação; remove o planejamento de rota até alvo e a permissão de disparar no chão ao usar WASD. Valida distância/linha de visão e acompanha o alvo apontado. ArenaInput aplica a permissão aos dois modos do mouse, preservando entrada de toque/gamepad. PlayerMotor.StopWalking zera somente velocidade de caminhada. A autoridade coop interrompe a inércia quando recebe ataque com movimento zero, depois de validar o comando. Não mudou o protocolo ou o schema dos saves.

No celular, joystick e botões permanecem independentes do mouse/Shift. As regiões locais passaram a reutilizar AimTargetSelector, com intervalo de 0,12 s, alvos vivos, alcance e linha de visão; a campanha online já tinha esse seletor. Botão SUPER também presente no HUD regional. Isso prepara controle por toque, não equivale a afirmar testes em todos os aparelhos.

Arquivos alterados: Scripts/Input/DesktopRpgControls.cs, ArenaInput.cs; Scripts/Gameplay/PlayerMotor.cs; Networking/CoopDirector.cs, World/RegionalWorld.cs, RegionalWorldHud.cs, Editor/CampaignInstaller.cs, Tests/ArpgControlsTests.cs; versão/configuração do player e documentação. Fontes pertencem a Assets/ForjaDasMares.

## Testes executados

Primeira rodada: 48/48. Rodada final ampliada: 54/54, zero falhas/ignorados. Inclui controles, Ato 1, região anterior, campanha, protocolo e seleção de mira. Casos específicos: alvo sem perseguição, retarget no mesmo pressionamento, saída do cursor sem caminhada, fora de alcance sem aproximar, WASD sem tiro no chão, alvo Training recebendo dano, Shift livre, UI bloqueando entrada, morte cancelando navegação, colisão e desvio de obstáculo. Asserções verificam dano real e deslocamento, não só estados visuais.

Windows 0.24.1: BuildPipeline Succeeded/errors0. Android solicitado explicitamente; celular identificado como moto g15, depuração autorizada, versão instalada antes da atualização 0.15.9/code40, apenas pacote com.forjadasmares.campaign encontrado. Build Android configurada 0.24.1/code48/ARM64/min25/target35. Resultado de geração, assinatura, instalação e teste físico será registrado no fechamento, sem considerar o APK antigo como novo.

## Como testar no PC

Na campanha ou na região, segure o inimigo com mouse: deve atacar sem caminhar. Saia do alvo mantendo o botão: não deve persegui-lo nem andar para o chão. Solte e clique no chão para reposicionar. Shift + mouse permite tiro livre parado. Verifique também alvo bloqueado, caixotes quebráveis, esquiva e alternância F8.

## Limites

Os testes automatizados são uma seleção focada, não garantia de ausência de todos os bugs do projeto. A região Ato 1 continua local. A campanha cooperativa existente mantém autoridade do host e as limitações documentadas anteriormente. Medições de FPS e testes físicos Android precisam ser descritos apenas conforme executados.


## Fechamento real

Rodada de entrega após ajustes móveis: 54/54 testes PlayMode, zero falhas/ignorados (`controls0241-release-tests.xml`). Builds Windows e Android finais: Succeeded/errors0/exit0. APK assinatura v2 verificada; instalação `adb install -r` Success no moto g15, mesmo pacote com.forjadasmares.campaign, versão0.24.1/code48. Não foi desinstalado nem limpo o armazenamento do aplicativo. Editor devolvido ao alvo Windows.

Conferência física: menu0.24.1, entrada na CostaAto1, joystick deslocando personagem/cenário, ataque segurado produzindo projétil real com rastro e botão INTERAGIR aceitando a missão de Nara. Caixa de diálogo passou do centro para canto superior esquerdo; textos de teclado e legenda sobre botões ocultos no celular. Capturas em Logs/Android0241. Trecho de log examinado sem Exception/FATAL/ANR; isso não é teste completo de todas as fases ou rede entre aparelhos nesta revisão.

Desempenho: amostra SurfaceFlinger de124 intervalos no refúgio físico anterior ao ajuste tinha média60,64ms (~16,5FPS), mediana65,79ms. Após otimização, amostra124 tinha média40,73ms (~24,6FPS), mediana32,99ms. São amostras curtas do SurfaceView, não benchmark controlado, nem garantia de30FPS sustentados. Continua importante perfilar combate/densidade e aquecimento no dispositivo.

Perfis móveis: Econômico renderScale0,55/MSAA1/30FPS alvo/sem pós; Equilibrado0,70/MSAA2/30FPS alvo; Nítido0,90/MSAA4/60FPS alvo. HDR desligado no móvel e distância de sombras12/22/32m. São metas e configurações, não FPS garantidos. A interface overlay conserva resolução nativa. PC preserva suas configurações anteriores. Em aparelho modesto, usar Econômico se o Equilibrado oscilar.

Ainda pendentes: teste físico de dano em inimigos/chefes com vários dedos simultâneos, sessão PC–Android desta revisão, quatro jogadores, perfil prolongado e otimização para30FPS sustentados. As provas físicas acima não devem ser apresentadas como todos esses testes concluídos.
