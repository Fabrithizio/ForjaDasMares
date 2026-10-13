# Etapa 4 — auditoria e plano de implementação

## Base auditada

Checkpoint c0bb83b, Unity 6000.3.26f1, URP 17.3.0, Input System 1.14.2. Cena ArenaTest existente e 23 testes aprovados na etapa 3. Código e árvore Git conferidos em clone independente; Windows e APK 0.2.0 testados, incluindo toque no moto g15. Cópia da cena antes desta etapa em work/etapa4/ArenaTest.before.unity. Nenhum Enemy/Boss/Encounter/Wave/Nav equivalente encontrado no código do jogo; não há AGENTS.md no projeto.

Conferidos Health, PlayerMotor, HeroCombat, DamageResolver, CombatProjectile, CombatEffectPool, ArenaSession, dados, assemblies, cena e documento mestre. A etapa anterior atende aos critérios funcionais verificados; porcentagem de certeza não substitui testes. Multitoque humano/gamepad físico e arte/clips finais continuam pendentes.

## Dependência de navegação

Usar AI Navigation 2.0.16, versão publicada para Unity 6000.3 na [documentação oficial](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.ai.navigation.html), mais o módulo AI integrado. A compatibilidade efetiva será confirmada pela resolução de pacotes e compilação local. NavMeshSurface será construído a partir do chão/obstáculos existentes; dados reais serão salvos no projeto. NavMeshAgent fornece caminho/desvio, CharacterController aplica movimento com colisões, usando a mesma função de gravidade/colisão do jogador. Não é necessário Visual Studio IDE.

## Componentes planejados

- EnemyDefinition: atributos, percepção, alcance, cadência, comportamento, bloqueio, invocação e referência de prefab/ataque.
- EnemyActor: ciclo de ativação/morte/retorno ao pool e referências explícitas de contexto.
- EnemyMotor: NavMesh, CharacterController, impedimentos/empurrão e detecção de falta de progresso; sem leitura de input.
- EnemyBrain: Idle/Patrol/Detect/Chase/Attack/Retreat/Stunned/Dead, distância/linha de visão e decisões com intervalos.
- EnemyCombat: preparação/resolução/recuperação, reutilizando AttackDefinition, DamageResolver, Health e pool de projéteis.
- EnemyPresentation e TelegraphPool: silhuetas geométricas, poses, áreas de perigo e bipes sintetizados originais. Nenhuma animação/modelo final será declarado pronto.
- EnemyPool: capacidade fixa por tipo, limite global de ativos e limites de invocação por proprietário; limpeza de referências na morte/desativação.
- EncounterDefinition/EncounterController: ondas configuráveis com tipos/quantidades/atrasos, eliminação/sobrevivência/chefe como condições, vitória e derrota reais.
- BossDefinition/BossBrain: pelo menos três ataques sinalizados (arco, marcas no chão, investida), fases e barra de vida. Recompensa será um contador de treino da sessão, concedido uma vez; não é inventário/progressão persistente.

## Integração e verificação

Preservar chão, luz, câmera, controles, heróis e arena. Acrescentar desafio selecionável/início de encontro ao HUD mínimo. R limpa encontro, projéteis, avisos, inimigos e restaura a arena. Trocar herói cancela o encontro de teste, evitando completar desafios restaurando vida artificialmente. Inimigos usarão camada própria e facções impedirão fogo amigo involuntário.

Manter os 23 testes anteriores. Acrescentar testes de dados/referências, navegação/obstáculos, alcance/visão, bloqueio frontal/traseiro, telegraph/áudio, dano único, invocação/capacidade/limpeza, ondas, chefe/fases/recompensa, vitória e morte. Exercitar cada herói contra cada um dos cinco tipos e contra o chefe. Compilar e testar no Windows/Android quando as ferramentas permitirem. Registrar limitações reais e parar ao final da etapa. Multiplayer permanece fora deste recorte.

## Fechamento PvE

35/35 testes PlayMode aprovados, zero falhas. As duas verificações adicionais confirmaram exatamente nove spawns nas três ondas e um único dano em área para o jogador com colliders múltiplos. O executável Windows 0.3.0 compilou, abriu com ARENA_RUNTIME_READY e PC_RPAsset, sem exceções no log. Captura URP real com três inimigos ativos revisada; a sobreposição do texto do encontro foi corrigida no HUD. Build final será recompilado com esse ajuste visual.

Etapa 4 validada no PC. Não houve teste físico Android nesta etapa; o telefone foi levado pelo usuário. Não há multiplayer, inventário ou arte final. Etapa 5 pode começar conforme autorização explícita de executar na ordem, sem confirmação presencial.
