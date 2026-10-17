# Campanha0.8.1 — solo, entrada em andamento e início

## Regras implementadas

- Todos os três mapas aceitam iniciar com1–4 jogadores. Criar sala privada e INICIAR SOLO funciona pela mesma autoridade de rede; não cria um segundo sistema de combate.
- Com exatamente um conectado, vida máxima e multiplicador de dano recebem ×1,10 após a fórmula normal de nível/equipamentos. Não altera defesa ou velocidade. Bônus temporário não é salvo nos itens. Ao entrar outra pessoa, sai imediatamente; ao restar uma pessoa, retorna. Mudanças preservam a proporção de vida, arredondando para baixo, para não curar por alternar entradas/saídas. Uma ação/projétil já lançado conserva os atributos daquele lançamento.
- Solo resolve placas visitando as duas em sequência e completando o tempo de ativação. Em grupo são necessários dois heróis distintos simultaneamente. Mudar entre solo/grupo zera progresso parcial; não reabre objetivos já concluídos.
- O host pode abrir/privatizar a sala tanto no mapa quanto durante a fase. A consulta busca a mesma fase/protocolo. Privada continua acessível por código enquanto novas entradas são permitidas.
- Nova entrada é aceita somente antes de20%: setores concluídos, excluindo a entrada, divididos pelos setores totais também sem a entrada. Ao atingir20% ou mais, a sala é bloqueada pelo anúncio e pela aprovação do host; corridas de consulta não contornam o limite. Limite4 inclui vagas reservadas em expedição. Reconexão de participante dentro da reserva continua sendo operação distinta.
- Quem chega carrega o mapa e recebe snapshot de portas, inimigos, vida, efeitos e objetivos. Não envia comandos até confirmar a assinatura do mapa. Aparece num ponto amostrado do NavMesh no setor atual; a partida dos demais continua. Tempo limite30s para confirmar mapa.
- Novo participante inicia uma transação própria e só recebe recompensas posteriores à entrada. Eventos anteriores já confirmados no ledger não são reaplicados ao recém-chegado. Não troca participante de uma vaga já usada na expedição.

## Interface inicial

Tela FORJA DAS MARÉS com CAMPANHA, AMIGOS E CONVITES, CONFIGURAÇÕES e saída confirmada. Configurações alteram volume real persistido e explicam controles. Campanha mostra iniciar solo/grupo, abrir/privatizar sala e progresso/solo no HUD. Inventário não aparece sobre a tela inicial.

AMIGOS E CONVITES é uma agenda local funcional de até6 nomes e códigos compartilhados: salvar e tocar no convite conecta à sala real. A interface declara explicitamente que códigos expiram e que não existe presença online/conta social global. Não é lista de amigos autenticados, chat ou envio remoto de convites. Isso requer uma etapa própria de identidade/serviço social.

## Arquivos

Novo CampaignHome.cs/.meta. Alterados CampaignHud, CoopCloud (visibilidade/anúncio/entrada em jogo), CoopDirector (aprovação, carregamento individual e bônus), CoopProgression/CoopRewardLedger (participante tardio), CoopProtocol4, CoopSmokeProbe, CoopInventoryHud, HeroEquipment, CampaignRun/CooperativePlates, CampaignInstaller e CampaignTests. Sem novos pacotes, segredos, modelos ou cenas recriadas. Preservados CampaignProfiles-v1 e variantes antigas.

## Verificação em andamento

Primeira rodada específica10/10 aprovada. Rodada ampla de rede32 casos:30 aprovados,1 ignorado (projeto já vinculado),1 falha no teste novo de navegação por procurar o HUD no objeto errado. Corrigida a referência do teste e a ocultação do botão de inventário no início. Repetição final específica11/11 aprovada. Compilações0.8.1 e testes ao vivo ainda em andamento neste checkpoint.

Não afirmar ainda vitória completa, duração equilibrada, desempenho sustentado, quatro dispositivos ou redes distintas. Sem migração de host ou save global entre hosts. A agenda de convites não substitui um serviço de amigos online.
