# Etapa 4 â€” auditoria e plano de implementaÃ§Ã£o

## Base auditada

Checkpoint c0bb83b, Unity 6000.3.26f1, URP 17.3.0, Input System 1.14.2. Cena ArenaTest existente e 23 testes aprovados na etapa 3. CÃ³digo e Ã¡rvore Git conferidos em clone independente; Windows e APK 0.2.0 testados, incluindo toque no moto g15. CÃ³pia da cena antes desta etapa em work/etapa4/ArenaTest.before.unity. Nenhum Enemy/Boss/Encounter/Wave/Nav equivalente encontrado no cÃ³digo do jogo; nÃ£o hÃ¡ AGENTS.md no projeto.

Conferidos Health, PlayerMotor, HeroCombat, DamageResolver, CombatProjectile, CombatEffectPool, ArenaSession, dados, assemblies, cena e documento mestre. A etapa anterior atende aos critÃ©rios funcionais verificados; porcentagem de certeza nÃ£o substitui testes. Multitoque humano/gamepad fÃ­sico e arte/clips finais continuam pendentes.

## DependÃªncia de navegaÃ§Ã£o

Usar AI Navigation 2.0.16, versÃ£o publicada para Unity 6000.3 na [documentaÃ§Ã£o oficial](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.ai.navigation.html), mais o mÃ³dulo AI integrado. A compatibilidade efetiva serÃ¡ confirmada pela resoluÃ§Ã£o de pacotes e compilaÃ§Ã£o local. NavMeshSurface serÃ¡ construÃ­do a partir do chÃ£o/obstÃ¡culos existentes; dados reais serÃ£o salvos no projeto. NavMeshAgent fornece caminho/desvio, CharacterController aplica movimento com colisÃµes, usando a mesma funÃ§Ã£o de gravidade/colisÃ£o do jogador. NÃ£o Ã© necessÃ¡rio Visual Studio IDE.

## Componentes planejados

- EnemyDefinition: atributos, percepÃ§Ã£o, alcance, cadÃªncia, comportamento, bloqueio, invocaÃ§Ã£o e referÃªncia de prefab/ataque.
- EnemyActor: ciclo de ativaÃ§Ã£o/morte/retorno ao pool e referÃªncias explÃ­citas de contexto.
- EnemyMotor: NavMesh, CharacterController, impedimentos/empurrÃ£o e detecÃ§Ã£o de falta de progresso; sem leitura de input.
- EnemyBrain: Idle/Patrol/Detect/Chase/Attack/Retreat/Stunned/Dead, distÃ¢ncia/linha de visÃ£o e decisÃµes com intervalos.
- EnemyCombat: preparaÃ§Ã£o/resoluÃ§Ã£o/recuperaÃ§Ã£o, reutilizando AttackDefinition, DamageResolver, Health e pool de projÃ©teis.
- EnemyPresentation e TelegraphPool: silhuetas geomÃ©tricas, poses, Ã¡reas de perigo e bipes sintetizados originais. Nenhuma animaÃ§Ã£o/modelo final serÃ¡ declarado pronto.
- EnemyPool: capacidade fixa por tipo, limite global de ativos e limites de invocaÃ§Ã£o por proprietÃ¡rio; limpeza de referÃªncias na morte/desativaÃ§Ã£o.
- EncounterDefinition/EncounterController: ondas configurÃ¡veis com tipos/quantidades/atrasos, eliminaÃ§Ã£o/sobrevivÃªncia/chefe como condiÃ§Ãµes, vitÃ³ria e derrota reais.
- BossDefinition/EnemyBrain: pelo menos trÃªs ataques sinalizados (arco, marcas no chÃ£o, investida), fases e barra de vida. Recompensa serÃ¡ um contador de treino da sessÃ£o, concedido uma vez; nÃ£o Ã© inventÃ¡rio/progressÃ£o persistente.

## IntegraÃ§Ã£o e verificaÃ§Ã£o

Preservar chÃ£o, luz, cÃ¢mera, controles, herÃ³is e arena. Acrescentar desafio selecionÃ¡vel/inÃ­cio de encontro ao HUD mÃ­nimo. R limpa encontro, projÃ©teis, avisos, inimigos e restaura a arena. Trocar herÃ³i cancela o encontro de teste, evitando completar desafios restaurando vida artificialmente. Inimigos usarÃ£o camada prÃ³pria e facÃ§Ãµes impedirÃ£o fogo amigo involuntÃ¡rio.

Manter os 23 testes anteriores. Acrescentar testes de dados/referÃªncias, navegaÃ§Ã£o/obstÃ¡culos, alcance/visÃ£o, bloqueio frontal/traseiro, telegraph/Ã¡udio, dano Ãºnico, invocaÃ§Ã£o/capacidade/limpeza, ondas, chefe/fases/recompensa, vitÃ³ria e morte. Exercitar cada herÃ³i contra cada um dos cinco tipos e contra o chefe. Compilar e testar no Windows/Android quando as ferramentas permitirem. Registrar limitaÃ§Ãµes reais e parar ao final da etapa. Multiplayer permanece fora deste recorte.

## Fechamento PvE

35/35 testes PlayMode aprovados, zero falhas. As duas verificaÃ§Ãµes adicionais confirmaram exatamente nove spawns nas trÃªs ondas e um Ãºnico dano em Ã¡rea para o jogador com colliders mÃºltiplos. O executÃ¡vel Windows 0.3.0 compilou, abriu com ARENA_RUNTIME_READY e PC_RPAsset, sem exceÃ§Ãµes no log. Captura URP real com trÃªs inimigos ativos revisada; a sobreposiÃ§Ã£o do texto do encontro foi corrigida no HUD. Build final serÃ¡ recompilado com esse ajuste visual.

Etapa 4 validada no PC. NÃ£o houve teste fÃ­sico Android nesta etapa; o telefone foi levado pelo usuÃ¡rio. NÃ£o hÃ¡ multiplayer, inventÃ¡rio ou arte final. Etapa 5 pode comeÃ§ar conforme autorizaÃ§Ã£o explÃ­cita de executar na ordem, sem confirmaÃ§Ã£o presencial.

