# Arena de teste — primeira versão jogável

A cena foi gerada pelo Editor e os 11 testes de PlayMode foram aprovados. Consulte PROGRESSO.md para os limites da validação.

## Local e dependências

Projeto: `C:/Users/Fabricio_S/Desktop/Jogos/ForjaDasMares`.

Editor escolhido: Unity 6.3 LTS, `6000.3.26f1`. Base: template oficial Universal 3D. O manifest e o lock do projeto registram as versões de pacotes efetivamente resolvidas após importação. Os alvos atuais são Windows e Android; iOS está adiado por instrução do usuário.

## Executar na Unity

1. No Unity Hub, adicionar esta pasta como projeto e abrir com a versão indicada em `ProjectSettings/ProjectVersion.txt`.
2. Aguardar a importação/resolução de pacotes e conferir o Console.
3. Abrir `Assets/ForjaDasMares/Scenes/ArenaTest.unity`.
4. Clicar em Play e dar foco à janela Game.

A cena já está salva e configurada. O menu `Forja das Mares/Create test arena (only if absent)` existe como ferramenta de recuperação: gera a cena somente se ela não existir e preserva uma arena já editada. Não é preciso executar um gerador a cada Play.

## Controles

| Ação | PC | Protótipo Android |
| --- | --- | --- |
| Mover | WASD ou setas, relativo à câmera | Arrastar joystick MOVER |
| Mirar | Mouse no plano do chão | Direção do movimento; orientação mantida quando para |
| Atacar | Clique esquerdo | ATAQUE |
| Esquiva | Espaço | ESQUIVA |
| Reiniciar | R ou botão REINICIAR | REINICIAR |

Gamepad tem bindings de movimento (analógico esquerdo), ataque (botão sul), esquiva (leste) e reset (Start). A integração é inicial; a interface final/remapeamento pertence a outra etapa. O protótipo Android usa paisagem. Toque funciona pelo mesmo Input System, por controles virtuais de gamepad, sem leitura de teclado no motor.

## O que observar

O personagem ciano é um placeholder composto de cápsula, esfera e cubos. O alvo laranja fica próximo do centro da arena. Aproximar-se, apontar e atacar reduz a vida em 25 por golpe válido, com flash branco e barra de vida. Após quatro golpes o alvo fica escuro; R restaura sua vida e devolve o personagem ao marcador de spawn.

A espada geométrica gira e um indicador geométrico de golpe aparece durante o ataque; são feedbacks procedurais, não uma animação final criada por artista. O alvo derrotado permanece como obstáculo de treinamento. Obstáculos e limites devem bloquear movimento e esquiva. Paredes bloqueiam o golpe.

## Ajustes no Inspector

- `Player_Placeholder/PlayerMotor`: velocidade, aceleração, desaceleração, gravidade, duração/velocidade/cooldown de esquiva.
- `Player_Placeholder/PlayerAim`: velocidade de giro e plano da mira.
- `03_MainCamera/ArenaCamera`: offset diagonal, ponto de observação, suavidade, máscara e raio de colisão, FOV e proporção de referência.
- `Data/TrainingSword.asset`: dano, alcance, arco, preparação e recuperação.
- `TrainingTarget_Placeholder/Health`: vida máxima.
- `TrainingTarget_Placeholder/TargetFeedback`: cor e duração do flash.
- `ArenaSession`: referências, spawn seguro, máscara de obstáculos e limite de queda.
- `05_PrototypeHUD/ArenaHud`: ativar `Show Touch In Editor` para inspecionar os botões de toque; isso não substitui um teste em aparelho Android.
- `Input/ArenaControls.inputactions`: ações Gameplay Move, Point, Attack, Dodge e Reset.

## Testes e limites

Os testes de PlayMode estão em `Assets/ForjaDasMares/Tests/PlayMode`. Usar Window → General → Test Runner → PlayMode. `PROGRESSO.md` deve informar quais foram realmente executados.

Esta entrega não inclui inimigos, multiplayer, progressão ou inventário. Também não demonstra desempenho em celular, crossplay ou prontidão comercial. Ao encontrar as paredes baixas desta arena, a câmera tenta uma inclinação mais alta mantendo o mesmo eixo horizontal, antes de reduzir sua distância. Em uma futura sala com teto/ambiente fechado, a estratégia precisa ser revalidada; espaços sem linha de visão livre podem reduzir o enquadramento. Arte, animações, áudio, safe area/remapeamento completos e interface final serão tratados em etapas posteriores.

## Android

Para um teste em aparelho, usar Android Build Support com o SDK/NDK/OpenJDK da mesma versão de Editor. No Build Profiles, criar/selecionar Android, incluir ArenaTest e conferir ferramentas externas em Preferences. A geração/instalação de APK e testes físicos só podem ser registrados como concluídos após execução real. Os módulos não substituem um telefone conectado/configurado.

