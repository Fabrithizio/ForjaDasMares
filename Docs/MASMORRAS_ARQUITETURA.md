# Masmorras — arquitetura implementada

Base auditada e reutilizada: arena/heróis e PvE, Unity 6000.3.26f1, URP17.3.0, Input System1.14.2 e AI Navigation2.0.16. Não havia sistemas equivalentes de Dungeon/Room/Biome/Interaction. DungeonTest foi salva a partir da cena existente, mantendo seus personagens, câmera, controles, pools e dados de combate. ArenaTest não foi regravada nesta etapa.

## Separação de responsabilidades

- DungeonLayout/DungeonNode: árvore lógica em grid; System.Random(seed), percurso principal fixo por categoria e coordenadas variadas; ramificações opcionais; assinatura, BFS e validação de conexões recíprocas/limite/overlap. Nenhum estado global de Unity Random participa do layout.
- RoomDefinition: categoria, tamanho de módulo, largura de porta, altura de paredes e obstáculos. O gerador usa módulos compatíveis de 12×12, corredor4×3 e grid16.
- BiomeDefinition: quatro materiais, iluminação, mesh/escala de decoração, tabelas de encontros e recompensa. Três assets originais, sem modelos/clips finais.
- DungeonGenerator: meshes explícitas, chão/peças/obstáculos/colliders, sockets cardinais, corredores, interações e navegação. Valida caminho do tamanho normal e do chefe antes de fechar as portas. Substituições de modelos devem preservar colliders/sockets e dados.
- DungeonRoom/DungeonDoor: estado da sala e barreiras físicas/carving dinâmico. A porta abre quando uma ponta foi concluída, ambas estão sem encontro ativo e a chave opcional permite a entrada. A sala de combate só conclui após fila de spawn e todos os inimigos relevantes mortos.
- DungeonSession: percurso, início de encontro sem restaurar o herói, objetivos obrigatórios, retorno/seed/bioma, chave/moedas voláteis, fim/reset. Trocar herói reinicia o percurso de teste.
- DungeonInteractable: distância validada, uso único, tampa/objeto geométrico, baú, cristal, descanso, alavanca e saída. Não há equipamento/inventário persistente.
- DungeonTrap: intervalo, aviso visual/bipe, dano único por pulso via DamageResolver e desarme. Não causa dano silencioso sem adquirir um aviso do pool.
- DungeonInteractionInput: asset próprio do Input System (F/botão norte) e comando do botão de toque, separado de movimento/mira/combate.
- DungeonHud: estado legível, vida, objetivo próximo, moedas/chave e barra do chefe. Renderização de salas atuais/adjacentes e corredores ligados à sala atual; câmera com limites opcionais somente nesta cena.

## Progressão de encontros

Os três encontros principais usam índices 0,1,2 da tabela de cada bioma: catacumbas usam saqueadores/guardas/arqueiro; floresta usa ecos/invocações; fortaleza usa instáveis/guardas/arqueiro. Atributos básicos dos inimigos não são multiplicados por profundidade. Quantidade/composição e recompensas são dados. O chefe reutiliza o Custódio do PvE.

## Verificação

Suíte de 41 casos aprovada, incluindo os 35 anteriores. 2.000 seeds × seis limites (12.000 combinações) conectadas, limitadas, reproduzíveis, com três salas de combate e chefe; 12 gerações físicas nos três biomas/quatro seeds, com NavMesh normal/chefe. Percurso completo venceu três combates, usou descanso, baú/cristal, desarmou a armadilha, matou chefe e saiu com movimento/colisões/combate reais. Verificações adicionais de câmera, input e alcance da investida do chefe estão na execução final registrada em PROGRESSO.md.

Sem teste físico Android nesta rodada: aparelho com o usuário. Sem alegações de profiling, balanceamento comercial, animações finais, salvamento completo ou multiplayer.
