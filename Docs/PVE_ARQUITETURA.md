# PvE — arquitetura e verificação

Base auditada: checkpoint c0bb83b, Unity6000.3.26f1, URP17.3.0, Input System1.14.2, ArenaTest e23 testes da etapa3. Nenhum sistema Enemy/Boss/Encounter/Wave/Nav equivalente existia. Preservados heróis, controles, chão, luz, câmera e combate.

AI Navigation2.0.16, publicada para Unity6000.3 na documentação oficial: https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.ai.navigation.html. Pacote/módulo AI resolvidos e compilados. Dois NavMeshData reais salvos: normal (raio0,5) e chefe (raio0,8/altura2,7). NavMeshAgent fornece caminhos e CharacterController aplica movimento/colisão por ActorMotion, compartilhado com o jogador. VS Code; nenhum Visual Studio IDE necessário.

EnemyDefinition configura atributos, ataques, percepção, bloqueio, invocação e prefab. EnemyActor cuida de ativação/morte/retorno ao pool. EnemyMotor separa navegação/movimento/empurrão. EnemyBrain tem Idle/Patrol/Detect/Chase/Attack/Retreat/Stunned/Dead e também seleciona os ataques/fases do chefe; não existe uma classe BossBrain separada. EnemyCombat controla aviso/resolução/recuperação e reutiliza AttackDefinition, DamageResolver, Health e CombatEffectPool. DirectionalGuard implementa IHitDefense.

EnemyPresentation usa geometria/poses provisórias e flash de dano. TelegraphPool reutiliza24 avisos e cria um clip PCM original com três bipes; não representa áudio final. EnemyPool pré-cria24 atores; limites padrão12 Android/18 PC, capacidade por tipo, máximo por invocador e orçamento de invocações. Morte limpa projéteis/avisos/invocações/referências.

EncounterDefinition tem ondas/tipos/quantidades/atrasos e condições. EncounterController aguarda fila e inimigos relevantes, trata vitória/derrota e recompensa do chefe uma vez. BossDefinition define arco, três marcas e investida; fase2 a50%HP. Recompensa são25 selos voláteis de treino, sem inventário/progressão persistente. R cancela/limpa; trocar herói cancela o desafio da arena. HUD tem encontros e barra real do chefe; HealthBarGeometry redimensiona o quad, sem depender de sprite ausente.

Verificação consolidada:45 testes aprovados, incluindo23 da base/heróis,14 PvE e8 masmorra. Cada herói venceu os cinco arquétipos e o chefe por ataques/habilidades reais. Foram testados navegação ao redor da parede, alcance/visão, bloqueio frontal/traseiro, explosão avisada/única, projétil único, limite/invocações/limpeza, ondas, morte/reset, três padrões do chefe, fase/recompensa e barra real. Ajustado alcance da investida de0,9 para1,5 para atingir além da margem dos colliders; aviso cobre a extensão total e acerto só frontal, uma vez por golpe.

Não foram feitos testes físicos Android nesta rodada porque o usuário levou o aparelho. Profiling, ergonomia, gamepad físico, balanceamento comercial, arte/clips finais e multiplayer continuam pendentes. Ver PROGRESSO.md e MASMORRAS_COMO_JOGAR.md para builds e limites reais.
