# Brasa: sobreposição, animação e ataque contínuo —0.7.1

## Problemas confirmados

O instalador anterior procurava um filho chamado Brasa_Model. SaveAsPrefabAsset salvou a raiz com o nome do arquivo Brasa; a busca pelo nome não encontrava a instância e cada execução acrescentava outra. O primeiro teste de unicidade encontrou4 HeroModelViews na revisão em andamento; a0.7.0 já havia recebido o instalador duas vezes. Os corpos ocupavam a mesma posição, mas apenas o primeiro recebia a apresentação do herói, produzindo sobreposição de poses.

Correção: identificar HeroModelView e confirmar o caminho do prefab antes de remover somente as instâncias redundantes comprovadas. Uma instância é mantida, renomeada explicitamente e validada. A apresentação também desativa visualizações extras ao selecionar o herói, como defesa para uma cena antiga. Placeholders referenciados permanecem serializados e desativados. Não foram apagados assets por parecerem desnecessários.

Outra disputa: HeroModelView chamava Animator.Play todo quadro durante o combate, além das transições AnyState. Agora a troca de estado tem um único dono; CrossFade ocorre ao mudar estado/versão do golpe, sem reinicialização a cada quadro. ActionTime controla o tempo dos clipes de ação e alinha a pose de contato ao instante real de dano. Esquiva e reação a dano também seguem seus relógios reais. Root motion continua desligado.

As prévias de poses também revelaram deformação do peito: pesos por proximidade de ossos deixavam regiões centrais sob influência dos braços/pescoço. Refeitos com transições anatômicas contínuas e peito ligado à coluna. Caminhada usa pernas com solução analítica de dois segmentos, apoio no chão e elevação somente do pé em balanço. Ciclos possuem endpoints iguais, avaliados e gravados. Ainda são animações autorais iniciais, sem mocap; rig, pesos e qualidade artística devem continuar sendo avaliados no jogo.

## Controles e regras

- PC: segurar mouse esquerdo mantém os ataques, com a mira existente. Clique curto continua atacando. O botão Ataque do HUD também aceita segurar. Gamepad conserva o ataque básico existente ao manter o botão pressionado.
- Android: segurar Ataque mantém os golpes; joystick continua independente. Um dedo controla o botão e soltar outro dedo não encerra esse ataque. Desativar controles, perder foco ou bloquear a entrada limpa a intenção.
- No modo Galerias, segurar por1,35s prepara automaticamente um golpe pesado na próxima janela válida. Ele causa2,4x o dano básico, arco mínimo160°, alcance corpo a corpo1,25x, contato em0,4s e duração0,8s. Intervalo mínimo3s entre pesados; não atravessa recuperação ou esquiva. Soltar antes de completar cancela o carregamento. Não é preciso clicar repetidamente nem soltar para disparar o pesado.
- Super:16 acertos válidos em inimigos enchem a barra. Não aumenta com golpes no vazio, dano direto de ferramentas de teste, alvos de treinamento, acertos bloqueados ou múltiplos colliders do mesmo alvo no mesmo golpe. Acertos intencionais distintos/projéteis e múltiplos inimigos contam separadamente.
- Q no PC, botão SUPER no Android ou ombro direito do gamepad solicita Ruptura de Brasa. Exige barra cheia e personagem vivo. Um buffer configurável de1,5s permite solicitar durante um golpe e iniciar após a recuperação; pedidos repetidos não acumulam supers. Bloquear a entrada limpa esse buffer. Consome toda a barra no início; impacto após0,45s, dano base120 e raio5,4, com knockback6 e teste de linha de visão. Recuperação0,9s. O próprio super não recarrega a barra e não concede invulnerabilidade.
- Barra de super e carregamento são temporários da tentativa; reiniciar/trocar de herói limpa esses valores. Não são moeda, item ou progresso permanente.

Os valores ficam em Data/MapStyles/Galerias/CombatFlow.asset e BrasaRupture.asset. O combate existente continua responsável por dano, obstáculos, alvos e deduplicação. Materiais/efeitos não concedem dano. Efeitos usam10 LineRenderers reutilizados por ator, com material serializado para inclusão na build, sem instanciar objetos por golpe ou alterar a câmera.

## Arte e integração

Doze clipes reais: Idle, Walk, Attack, Attack2, Attack3, Charged, Skill, Super, Dodge, Hit, Dead, Victory. Corpo/esqueleto/FBX/prefab/controller e fonte Blender atualizados. Sem criar outro herói lógico ou duplicar o controlador do jogador. Demais classes mantêm seus visuais provisórios; o perfil de fluxo local pode ser reaproveitado sem alterar a definição de suas armas.

Recorte: modelo corrigido, carregado e super instalados em GaleriasDungeon/GaleriasArena. O cooperativo conserva seu protocolo e visuais anteriores; recebe a intenção de ataque básico enquanto o botão estiver pressionado. Carregado/super novos ainda não são habilitados, sincronizados ou declarados testados na rede. Não há alteração de economia ou inventário e nenhum serviço/credencial novo.

## Arquivos

Novos: CombatFlowDefinition.cs, IContinuousCombatCommands.cs, HoldAttackButton.cs, HeroPowerFeedback.cs, ContinuousCombatTests.cs, seus metas, CombatFlow.asset, BrasaRupture.asset, CombatRibbon.mat e este relatório. Alterados: create_brasa.py, .blend/preview/model_report, FBX/meta/prefab/controller, BrasaModelInstaller, GaleriasInstaller, HeroModelView, HeroPresentation, HeroCombat, Health, PlayerMotor, ArenaInput, RusticDungeonHud, HeroHud, CoopDirector, BrasaModelTests, CoopProgressionTests, duas cenas Galerias e versão/documentação. Pacotes, mapas, saques e arquivos de salvamento preservados.

## Como verificar na Unity

Abrir GaleriasDungeon e Play. Inspecionar HeroVisual_warrior: deve existir exatamente um HeroModelView e um corpo skinned ativo, com a espada separada. Andar, parar, atacar, esquivar, receber dano e trocar/equipar/desequipar heróis. Segurar Ataque por alguns segundos para observar combo e pesado. Enfrentar inimigos até preencher a barra; usar Q/SUPER, conferir consumo, área, dano e bloqueio por paredes. Reiniciar confirma limpeza das barras. Reexecutar Forja das Mares/Characters/Install Brasa in Galerias deve conservar uma única instância.

## Execuções desta revisão

Primeira suíte ampla:122 casos,116 aprovados,5 falhas,1 ignorado Cloud. As falhas revelaram a duplicação real, uma espera do teste de transição menor que a duração da transição, dois testes novos que desligavam o próprio componente responsável por observar acertos e uma falha no percurso cooperativo já intermitente na0.7.0. Os problemas novos foram corrigidos; rodadas posteriores e builds serão registradas abaixo somente após execução. Os116 aprovados não equivalem a aprovação final da revisão.

Após as correções:40/40 focados aprovados; repetição ampla123 casos com122 aprovados/0 falhas/1 ignorado (UnlinkedInternetAttemptPreservesOfflineSession requer projeto não vinculado; este projeto está vinculado). Depois do buffer do super,42 focados tiveram41 aprovados/1 falha no mesmo percurso cooperativo, sala4. Inspeção do teste mostrou o robô seguindo em linha reta através de caixas. Alterado somente seu deslocamento para usar caminhos reais do NavMesh quando a linha de visão está bloqueada, mantendo colisões, dano, vida, saques, conclusão e todas as asserções. Nova rodada42/42 aprovada; logs COOP_TEST_BOT_OBSTACLE_PATH registraram caminhos de5 cantos nas salas1/2/4, incluindo a sala problemática. Isso melhora o robô de regressão; não constitui prova de estabilidade universal do PvE ou de teste online com vários dispositivos.

Último refinamento evita fitas de ataque corpo a corpo com alcance enorme nos heróis de projéteis; eles preservam seus projéteis reais. Rodada final específica de modelo/combate/HUD/heróis:31/31 aprovados, incluindo visual do super com10 fitas/material válido/sem colliders e projéteis sem fitas indevidas. Evidências locais: work/blender-character/combat-final-regression.xml, combat-final-focused.xml, combat-accepted.xml, combat-effects-final.xml. As rodadas são distintas; a suíte ampla não foi repetida após os últimos refinamentos. Permanecem avisos nativos JobTempAlloc em batch, já registrados anteriormente, sem alegação de correção nesta entrega.
