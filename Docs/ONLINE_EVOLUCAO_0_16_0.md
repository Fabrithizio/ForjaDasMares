# Evolução online — pontos de migração existentes

Este documento prepara a próxima arquitetura. Não instala servidor dedicado, contas globais nem migração de host.

## O que existe
- `CoopDirector.Accept` recebe entradas finitas, ordenadas e limitadas; `HeroCombat`, `DamageResolver`, encontros e recompensas executam na autoridade atual.
- NGO/Unity Transport e os serviços já configurados mantêm a sessão. Relay transporta mensagens; não passa a simular o jogo nem a guardar a sala quando o host fecha.
- `CoopProfileChannel` transporta intenções; `ProfileService` valida compatibilidade, ações e transações. Perfis locais e espelhos de sala não equivalem a uma conta online comercial.
- `CampaignSessionMenu` explica a saída do host e fornece código/convite. Preservar o encerramento seguro atual enquanto não houver outra autoridade real.

## Migração incremental proposta
1. Extrair a inicialização de uma sessão sem interface a partir de CoopDirector e da mesma simulação de combate/mapa; validar uma build de servidor com módulos de plataforma instalados antes de prometer disponibilidade.
2. Substituir a atribuição de vagas locais por autenticação verificável. Vincular personagem/progresso ao ID de conta emitido pelo serviço, sem aceitar um ID arbitrário do cliente como prova de posse.
3. Levar persistência de ProfileService para armazenamento controlado pelo servidor, com versão e transações idempotentes por jogador/run/recompensa. Importação de saves locais precisa de regra explícita; não aceitar ouro/itens locais como economia online confiável.
4. Manter a geração/seed e os resultados na autoridade. Reutilizar catálogos e snapshots existentes; separar versão de conteúdo, protocolo e save.
5. Alocar servidores por região/capacidade com credenciais somente no backend. Cliente recebe credencial curta de entrada, nunca chave administrativa.
6. Tratar ausência/retorno de jogadores sem encerrar a sala; definir tempo de expiração da instância e gravação do resultado. Isso é diferente de migrar automaticamente o host atual.
7. Testar estado compartilhado, exploração de entradas inválidas, desconexão/retorno, reinício da instância, duplicação de loot e custos/capacidade em sessões reais.

## Limites atuais
- Um host modificado ainda pode manipular sua própria autoridade. Não há alegação de economia segura para lançamento.
- Não há presença global de amigos ou sala independente do host implementada neste bloco.
- Nenhum novo serviço, cobrança ou segredo foi configurado. Antes de contratar infraestrutura, confirmar preços/limites diretamente no provedor e definir orçamento/telemetria.
- Testes com seu irmão, aparelhos reais e redes diferentes continuam necessários; testes locais não os substituem.
