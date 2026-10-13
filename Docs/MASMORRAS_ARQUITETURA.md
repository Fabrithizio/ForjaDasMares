# Etapa 5 — plano aguardando entrega PvE

Auditoria: nenhuma implementação equivalente de Dungeon/Room/Biome/Loot/Interaction encontrada nos scripts existentes. ArenaTest permanecerá como referência. A nova cena DungeonTest reutilizará jogador/câmera/UI/EncounterController/pools e dados da etapa 4. Não reconstruir os heróis ou dano.

Layout lógico de árvore em grid com espaçamento 16, salas 12x12, corredores 4x3, sockets cardinais recíprocos. System.Random(seed), percurso principal entrada→combate→combate→evento→combate difícil→armadilha→chefe. Ramificações de tesouro e descanso até o limite configurado. Tentativas limitadas + fallback fixo validado; BFS, coords únicas, portas recíprocas, saída final e limite. Testar milhares de seeds e mesma seed igual assinatura. Mapa fixo preserva a mesma sequência de teste.

BiomeDefinition: materiais/paleta, luz, decoração, tabela de encontros e recompensas; três assets. RoomDefinition: categoria, dimensões, obstáculos locais, sockets e objetivos. Construtor usa meshes explícitas (sem Runtime CreatePrimitive, evitando stripping Android). Salas/controladores gerados têm dados e referências completas, sem métodos vazios.

NavMeshSurface normal+chefe gerados depois da geometria, portas abertas durante bake; portas com colliders + carving dinâmico. Portas de progresso só abrem após vencer encontros e objetivos; fechar entrada quando herói passar a margem segura, não dentro do collider. Spawn usa pontos internos válidos + validação existente do EnemyPool, excluindo paredes, baús e jogador.

Interações F/gamepad/celular: baú com abertura geométrica, chave de sessão opcional para tesouro, evento obrigatório, descanso cura única, alavanca de armadilha, saída após chefe. Saldo/chaves voláteis, sem equipamentos/inventário persistente. Armadilhas reutilizam Health/DamageResolver/TelegraphPool e têm aviso/intervalo/ledger de acerto.

Dificuldade por composições e quantidade de encontros por profundidade; atributos de inimigos preservados. RoomController exige fila de spawn concluída, todos relevantes mortos e objetivo obrigatório. R reinicia mesma seed; troca de herói reinicia o percurso de teste para não conceder vitórias falsas. UI escolha de bioma/seed/fixo e volta à arena. Mostrar só salas atuais/adjacentes e fundo escuro; câmera com limites opcionais exclusivamente na DungeonTest.

Validar conectividade lógica e NavMesh real, portas/corredores/baú/objetivos/armadilha/chefe/saída; executar sequência completa por combate existente. PC executável e captura; Android sem aparelho nesta rodada. Nunca declarar arte final, teste físico ou multiplayer.
