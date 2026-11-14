# Análise de clareza e funcionalidade — base 0.15.0

## Escopo e evidência

Revisão estática do código atual, dos fluxos de interface e da documentação da última entrega. Nenhum script, dado, cena, build ou save alterado. Não foi executada nova sessão interativa, compilação ou teste físico nesta análise. O relato do jogador sobre tiro e equipagem é a referência de comportamento; causas específicas ainda devem ser reproduzidas com o seu cenário. Os testes anteriores não cobriam toda a combinação de movimento, mira, projéteis ativos e ações pela interface.

## Problemas encontrados no código

### 1. Equipagem: interface e autoridade discordam — prioridade máxima

CoopInventoryHud.CanManageNow considera fase, vida, encontro e ameaça em 12m. CoopProgression.CanManage adiciona combate ocupado e ActiveProjectiles == 0 / ActiveZones == 0 do pool compartilhado. Um projétil ou área de outro ator pode impedir a gestão de equipamentos de quem está seguro. CoopProfileChannel.Apply retorna sem explicar quando CanManage falha. É uma inconsistência confirmada; não prova que seja a única causa do caso relatado.

A comparação também pode impedir equipagem por classe, nível, mão secundária, arma de duas mãos ou item equipado em outro herói. O popup mostra alguns motivos via ItemDescriptions.Preview, mas o bloqueio de gestão não tem explicação junto ao botão. Parte das mensagens é escrita no detalhe legado, cujo viewport fica desligado no popup atual.

Correção proposta: uma regra central de permissão com motivo; botão e autoridade devem refletir o mesmo estado; validar ameaças/ações do jogador, sem bloqueio por efeitos irrelevantes dos aliados; mostrar a recusa junto da ação e confirmação do servidor. Não remover validação de segurança do host.

Fontes: Networking/CoopInventoryHud.cs (CanManageNow/Refresh), Networking/CoopProgression.cs (CanManage), Networking/CoopProfileChannel.cs (Apply), Scripts/Gameplay/ProgressionStats.cs (CanEquip).

### 2. Mira e animação do Arcanista — hipótese técnica forte

HeroCombat.Begin guarda facing = transform.forward no início. ResolveBasic usa essa direção guardada, mas lê origin.position no instante do disparo. O emissor acompanha o ator, e a rotação é aplicada por CoopDirector.Accept quando FacingLocked termina. São estados atualizados em momentos distintos; com movimento e mudança de mira, origem, pose e direção podem divergir. No celular Aim escolhe o mais próximo a cada atualização, sem retenção explícita de alvo, permitindo trocas repentinas entre inimigos próximos.

Correção proposta: definir uma política única para a direção válida de cada disparo, alinhar indicador/pose/projétil no instante de lançamento, preservar validação de obstáculos e movimento independente da mira. No toque, retenção e troca previsível de alvo. Reproduzir antes de escolher a correção exata; não atribuir o caso só à animação ou só à rede.

Fontes: Scripts/Gameplay/HeroCombat.cs (Begin/ResolveBasic), Networking/CoopDirector.cs (Aim/Accept), Scripts/Gameplay/PlayerMotor.cs (LockFacing/Simulate).

### 3. Ataque segurado reúne ações pouco explícitas

ProcessAttackInput combina repetição do básico, carga automática, pesado automático e soltura antecipada a partir de 65% com buffer. Isso pode mudar o ritmo sem o jogador entender por quê. Tornar visíveis a preparação, a carga e a recuperação, e explicar claramente o que segurar/soltar faz. Decidir o comportamento final antes de alterar controles.

### 4. Identificação e comparação de itens

A descrição tem classe, slot, requisitos, sequência inteira, efeitos, sinergia e comparação geral. O problema não é falta absoluta de dados: é a prioridade e o vocabulário. Há ainda "alvos em área" derivado de maximumTargets == 0 para projéteis; esse valor não prova explosão, pois splashRadius pode ser zero. Cada categoria usa ícone compartilhado, dificultando distinguir itens da mesma categoria.

Proposta: resumo imediato com ícone, nome, tipo/slot, classe, requisito, estado e três mudanças principais. Detalhes avançados expansíveis. Descrever alvo único, dispersão e explosão conforme o comportamento real. Distinguir equipado neste herói, em outro herói, incompatível e bloqueado temporariamente.

Fontes: Scripts/UI/ItemDescriptions.cs; Networking/CoopInventoryHud.cs; Scripts/Gameplay/InventoryRules.cs.

## Revisões de experiência propostas — não são bugs comprovados em execução

1. **Mochila e personagem:** acesso único; equipamento ao redor do avatar; identificar banco, saque da tentativa e entregas sem exigir descobrir três lugares. Preservar a separação técnica e os itens salvos.
2. **Tela inicial e campanha:** hierarquia clara entre Personagem, Campanha e jogar com alguém. Seleção de herói por cartões identificáveis em vez de um botão que percorre classes.
3. **Entrar numa partida:** após escolher a fase, caminhos claros para jogar sozinho, procurar grupo e entrar com código. Criação de sala privada fica dentro do fluxo de grupo. Expor carregamento, tentativa de conexão, sala cheia e recuperação de falha.
4. **Perfil e salvamento:** tornar explícito qual perfil está sendo editado e onde o progresso será guardado. Hoje existem perfil deste aparelho e cópia da última sala; trocar contexto pode parecer perda de itens. Unificar a experiência exige respeitar a autoridade e a persistência atuais; conta global/servidor é outro trabalho.
5. **Mapa e objetivos:** destacar um próximo objetivo, localização dos inimigos pendentes e destino de interação. Verificar o retorno a um encontro anterior sem depender de procurar inimigos pelo mapa inteiro.
6. **Leitura do combate:** diferenciar efeito de aliado, perigo inimigo e loot; tornar claros alcance, impacto, recarga e dano recebido. Áreas coloridas de protótipo ainda podem esconder personagens ou parecer perigos indevidos.
7. **Balanceamento:** medir tempo para matar/morrer, disponibilidade de cura, experiência, frequência de upgrades e contribuição de cada arma. Ajustar depois da mira e equipagem para não compensar bugs com números. Cada mudança deve ter alvo e comparação de antes/depois.
8. **Celular e desempenho:** testar botões alcançáveis, leitura em tela pequena, mira automática, navegação e menus durante combate. Medir CPU/GC/GPU/latência antes de atribuir travadas a um sistema. Não há medição nova nesta análise.

## Ordem sugerida dos blocos

1. Equipagem funcional + motivo visível de qualquer bloqueio.
2. Mira/disparo do Arcanista parado e em movimento, PC e toque.
3. Identificação dos itens + mochila/comparação.
4. Início → personagem → campanha → entrar/jogar.
5. Objetivos/mapa + leitura do combate.
6. Balanceamento medido e desempenho em aparelhos disponíveis.

Cada bloco deve terminar com comparação visual e teste funcional reproduzível. Sem adicionar conteúdo novo durante esses blocos.

## Aceitação dos primeiros blocos

- Item compatível equipa no início, lobby e situação permitida em partida; o host confirma a mudança, os atributos e o visual refletem o resultado, e a alteração persiste.
- Item incompatível ou ação temporariamente proibida informa o motivo exato no local da ação. Um aliado disparando longe não deve impedir troca segura sem uma regra deliberada e comunicada.
- Repetir tiro do Arcanista parado, andando em oito direções, virando, junto a obstáculos e como cliente. O indicador e o projétil devem concordar; dispersão intencional deve ser identificável. No toque, o alvo não muda de forma arbitrária.
- Uma pessoa consegue localizar a fase, iniciar sozinha/entrar com código e identificar item/slot sem orientação externa. Manter atributos, sinergias e builds para decisões avançadas.

## Situação

Análise entregue para definir a primeira correção. Nenhuma correção implementada nesta revisão. Equipagem e mira devem preceder novos ajustes numéricos. Não é possível garantir funcionamento perfeito por leitura de código; a aprovação de cada bloco depende dos testes dos fluxos reais.
