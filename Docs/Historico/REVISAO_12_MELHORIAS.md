# Revisão das 12 melhorias — 26/10/2026

Referência original: MELHORIAS_PRIORIZADAS_0_15_9.md. Implementação principal: CLAREZA_0_16_0.md. Não confundir preparação, teste automatizado e validação humana.

| Ponto | Estado real | Falta |
|---|---|---|
| 1. Inventário PC | Implementado, conferido e coberto por regressão | Polimento iterativo |
| 2. Inventário de toque | Código e layout implementados, conferidos em janela pequena | Teste físico da versão atual; sem APK novo por preferência do usuário |
| 3. Identificação de itens | Slot, classe, compatibilidade, requisitos e efeitos descritos | Ícones exclusivos para cada peça/variante não existem |
| 4. Leitura das builds | Comparação com equipamento do mesmo slot e fórmulas reais | Verificação humana prolongada de escolhas/builds |
| 5. Arcanista em movimento | Correção de direção no lançamento e testes em oito direções | Nova avaliação humana após animações da Sapphire |
| 6. Animações e impactos | Efeitos existentes, skin nova e ajuste de escalas | Nova skin só tem pose de repouso; precisa de clips compatíveis, não está concluído |
| 7. Primeiros passos | Guia funcional, dispensável e reabrível | Validação com jogador novo |
| 8. Ritmo e dificuldade | Limites auditados e entrada de inimigos ajustada | Partidas humanas medidas solo/dupla; não declarar balanceamento final |
| 9. Orientação | Minimapa com inimigos/distância e avisos de entrada | Revisão artística de marcos e legibilidade durante sessões |
| 10. Sessão com irmão | Testes anteriores em dois processos PC/Relay | Não foi jogada uma sessão com o irmão; nem PC–Android atual. Reconexão direta abrupta falha |
| 11. Evolução online | Plano incremental escrito e autoridade atual preservada | Conta global, servidor dedicado e migração não implementados; não eram prometidos como deploy nessa lista |
| 12. Desempenho | Diagnóstico local, qualidade e correção da amostragem | Medição prolongada em hardware real, CPU/GPU separadas e Android |

## Trabalho desta rodada — 0.16.3

Correção comprovada no CampaignPerformanceHud: a janela de FPS/p95 acumulava quadros de lobby/inventário, apesar de só exibir o painel em jogo. Agora coleta apenas em Playing, com inventário e menu da sessão fechados, e reinicia a janela ao sair dessas condições. Não representa otimização de FPS nem medição separada de CPU/GPU.

Novo teste verifica inventário → lobby → partida → inventário, conferindo a coleta e a limpeza da amostra. Primeira rodada: 12/13; o novo teste procurava o componente no objeto errado (o diagnóstico pertence ao HUD). Corrigida a busca no teste. Segunda rodada: 13/13 aprovados, zero ignorados. XML local Logs/finish12-tests.xml. Não foi executada novamente toda a suíte geral.

Reconexão direta novamente testada com dois processos Windows 0.16.2 sem renderização, perfis de QA isolados: ambos chegaram a Playing, 2 participantes, mapa compartilhado. Cliente encerrado à força; host permaneceu em Playing com 1 participante. Nova instância com a mesma identidade/porta/código terminou Closed por falha de conexão no transporte. Sem causa comprovada, sem alteração especulativa no Netcode. Evidências locais em Logs/Review0163 e review0163-*.log, excluídas do Git. Esse teste não foi executado pelo Relay nem em Android. Uma tentativa inicial com IDs inválidos foi descartada e repetida com IDs válidos.

Requisitos externos: obter clips conforme PERSONAGEM_ANIMACOES.md; realizar teste humano com irmão e disponibilizar aparelho quando desejar a rodada móvel. Não usar essas dependências para declarar a lista 100% pronta. Nenhuma compra, credencial ou novo serviço foi criado.

Arquivos alterados: CampaignPerformanceHud.cs, InventoryClarityTests.cs, CampaignInstaller.cs, ProjectSettings.asset e documentação. Sem novo APK. Caminho do executável Windows preservado em Builds/Atual/Windows/ForjaDasMaresCampanha.exe.

Build Windows 0.16.3: Succeeded/errors=0/exit=0; executável estável atualizado.
