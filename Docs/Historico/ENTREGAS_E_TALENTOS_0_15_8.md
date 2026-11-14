# Capacidade, entregas em lote e talentos — 0.15.8

## Capacidade da mochila
A campanha mostra quantidade/limite no topo e no botão MOCHILA (ex.: 30/36), com CHEIA quando não há vagas. O limite vem do catálogo; categorias continuam sendo filtros do mesmo espaço, e itens equipados não ocupam vagas. O texto da mochila explica essa regra.

## Receber todos
Botão funcional na aba ENTREGAS, com quantidade que cabe. Recebe todas as categorias, independentemente do filtro, inclusive entregas além das oito primeiras replicadas ao cliente. O host calcula espaço/quantidade e move as instâncias em uma transação salva. Sem autoequipar, criar cópias ou conceder moedas. Se couber apenas uma parte, recebe essa parte e mantém o resto em Entregas com mensagem. Sem vagas/entregas, em estado sem permissão ou no cache de outra sala, o botão fica indisponível; a cópia remota continua exigindo voltar à origem.

CoopInventoryAction.ClaimAllDeliveries foi acrescentado ao final do enum e roteado no host e no perfil local. Protocolo elevado de10 para11 para não permitir uma sessão com versões que desconhecem a operação. Todos os participantes precisam da0.15.8 ou de versão posterior compatível. Savev2 e IDs dos itens/talentos preservados.

## Talentos: estilo escolhido
Baseado na quinta referência: herói/resumo à esquerda, lista rolável no centro e detalhe à direita. Essa organização suporta aumentar a lista sem fixar posições de uma árvore circular. Usa o retrato 3D real do herói e a textura de preview já existente, sem criar outro personagem/câmera de jogo.

- Ícones vetoriais originais coloridos, seleção destacada, rank e barra por talento.
- Descrição, contribuição atual do talento, contribuição no próximo rank e valor por ponto separados dos atributos totais do herói.
- Rank máximo mostra MÁXIMO e bloqueia a compra; não exibe um rank impossível6/5.
- Melhorar continua enviando intenção validada, consome um ponto por compra e atualiza a ficha.
- Redistribuir mantém custo real e exige segundo clique de confirmação.
- Retorno de operação fica no rodapé da tela; mensagens da mochila são ocultadas atrás do modal.
- TalentRule admite Sprite e descrição opcionais no catálogo. Sem arte atribuída, os ícones vetoriais e descrições por atributo continuam funcionais; textos extensos têm área rolável.

O conteúdo continua sendo os seis talentos existentes. O layout percorre o catálogo e tem rolagem, mas novos IDs, habilidades ativas e alocações adicionais ainda exigem implementar os dados/comportamentos e a migração adequada: a persistência atual possui seis posições. Não foram criadas habilidades fictícias nem mudados os bônus/custos para copiar os números das referências.

## Arquivos
- Scripts/Gameplay/ProfileService.cs — transação ClaimAllDeliveries.
- Networking/CoopProfileChannel.cs e CampaignProfileHub.cs — intenção e rotas autoritativas/local.
- Networking/CoopInventoryHud.cs — capacidade, botão, estado de recebimento e preview compartilhado.
- Networking/CampaignCharacterSheet.cs — nova tela e ações de talentos.
- Scripts/Gameplay/HeroTalents.cs — metadados visuais opcionais.
- Scripts/UI/HudIcon.cs — coração, alvo e ampulheta; valores antigos do enum preservados.
- Networking/CoopProtocol.cs — protocolo11.
- Networking/Editor/CampaignInstaller.cs e ProjectSettings/ProjectSettings.asset — versão0.15.8, Android código39.
- Docs/PROGRESSO.md e ARQUITETURA.md — registro e contrato atualizado.

## Conferência efetuada
Primeira build Windows Succeeded/errors0/exit0. Conferência da interface real em cópias isoladas de perfil, com -netSavePath e -netIdentity, sem distribuir pontos, receber itens ou gastar ouro no perfil normal:
1. Mochila34/36 e duas entregas: um clique recebeu duas, ficou36/36 e zero entregas; todos os IDs preservados e sem duplicação.
2. Mochila35/36 e duas entregas: recebeu uma, ficou36/36 e uma entrega; todos os IDs preservados e sem duplicação; botão bloqueado sem vagas.
3. Vigor0/5→1/5, pontos2→1, vida165→173.
4. Força5/5 bloqueou compra e mostrou MÁXIMO.
5. Lista rolou até Disciplina.
6. Redistribuir primeiro pediu confirmação; depois descontou175 ouro (1333→1158), devolveu os ranks e mostrou8 pontos livres.
Evidência local sem publicar saves: Logs/Review0158/results.json. Nenhum dos padrões NullReferenceException/MissingReferenceException/ArgumentException/Cannot add foi encontrado no log da primeira conferência.

Build final Windows Succeeded/errors0/exit0 (Logs/talents-0158-pc-final.log). Geração Android interrompida por solicitação do usuário; não há APK0.15.8 entregue e não houve teste físico Android. O desenvolvimento continua compatível com o código compartilhado; empacotamento móvel somente quando solicitado. Não foram criados/rodados novos testes automatizados ou uma sessão online nova. Operação de rede revisada em código; falta exercitar a nova intenção com dois clientes reais.

## Como abrir/configuração
Abra a campanha e PERSONAGEM E EQUIPAMENTOS. Veja o contador; abra ENTREGAS → RECEBER TODOS. Para talentos, use ATRIBUTOS E TALENTOS, selecione uma opção na lista e confira o painel antes de clicar MELHORAR. Role para chegar às opções inferiores. Não há pacote novo ou configuração manual obrigatória. Para arte futura, configure icon/description nos dados do talento; os recursos atuais continuam sendo vetores e modelos existentes.

Preferência de trabalho registrada em AGENTS.md: PC primeiro; APK somente quando solicitado. Editor devolvido ao alvo Windows sem gerar outra build (Logs/restore-windows-0158.log, exit0).
