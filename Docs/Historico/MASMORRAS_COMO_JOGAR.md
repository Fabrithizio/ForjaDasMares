# Masmorras — versão 0.4.0

Abra Assets/ForjaDasMares/Scenes/DungeonTest.unity e pressione Play, ou use Forja das Mares → Open dungeon test. ArenaTest continua preservada para testes de heróis/PvE (F6). O build Windows inclui as duas cenas e inicia na masmorra; VOLTAR A ARENA abre a referência.

## Controles

WASD/setas ou joystick: movimento; mouse/clique esquerdo ou ATAQUE: ataque; E/clique direito ou HABILIDADE: especial; Espaço/ESQUIVA; 1–4/Tab ou HERÓI: seleção. Trocar herói reinicia o percurso de teste, evitando recuperação de vida no meio de um encontro. R/REINICIAR refaz a mesma seed. F, gamepad botão norte ou INTERAGIR: usar objeto próximo. A janela Game precisa ter foco para teclado/mouse.

TROCAR BIOMA alterna catacumbas, floresta subterrânea e fortaleza vulcânica. FIXO / PROCEDURAL muda o modo; NOVA SEED incrementa a seed. Seed arbitrária e limite 7–12 salas são ajustáveis no componente DungeonSession, Inspector, antes do Play. Mapa fixo usa um percurso de referência; variar seed nesse modo muda a recompensa, não o percurso principal.

## Percurso de referência

Entrada → combate → combate → evento (baú + cristal) → combate difícil → armadilha (alavanca) → chefe → saída. Descanso opcional conectado à segunda sala de combate cura uma vez. Tesouro opcional conectado à primeira sala precisa da chave obtida no cristal; a chave permanece válida durante a expedição e libera ramificações de tesouro. É possível voltar por salas concluídas.

Portas se fecham quando o jogador entra em uma sala de encontro. Só abrem depois da fila de spawn e da morte de todos os inimigos relevantes. Eventos exigem ativar o cristal; armadilha exige desligar a alavanca. O baú é opcional, concede moedas temporárias uma vez e abre a tampa geométrica. A saída só funciona depois de derrotar o chefe. Contadores de moedas/chave/selos são temporários: não existe inventário completo, equipamento ou salvamento nesta etapa.

Áreas coloridas e bipes avisam ataques/armadilhas antes do dano; esquive ou saia da área. Obstáculos têm colisão e exigem contornar. Derrota exige reiniciar. Caminhe até 2,1 unidades de um objeto para interagir; aparece uma mensagem perto da parte inferior da tela.

## Dados e configuração

Data/Dungeons contém três BiomeDefinition, oito RoomDefinition, tabela de recompensas, ataque de armadilha e nove tabelas de encontros. A dificuldade é a composição/quantidade de inimigos por profundidade; os atributos dos inimigos permanecem nos dados PvE. Configurações de sala usam módulos de 12×12, corredor de 4×3 e grid de 16; alterações de dimensões precisam manter essa compatibilidade no gerador. Sockets cardinais e colliders são criados de forma explícita.

DungeonGenerator recebe meshes/materials e superfícies de navegação normal/chefe já configuradas na cena. Ao gerar, constrói geometria e NavMesh com portas abertas, valida caminhos de ambos os tamanhos e então aplica bloqueios dinâmicos. Não crie prefabs/modelos faltantes manualmente: os placeholders são meshes geométricas reais configuradas no projeto. Câmera recebe limite de foco por sala; renderiza salas atuais/adjacentes, com fundo escuro, mantendo os colliders do percurso.

Android preserva o joystick corrigido e acrescenta INTERAGIR. O aparelho está com o usuário: esta entrega não alega testes físicos Android, desempenho em celulares fracos ou crossplay. Arte e animações continuam provisórias. Multiplayer e equipamentos não foram implementados.
