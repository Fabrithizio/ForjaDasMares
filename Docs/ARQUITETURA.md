# Atualização de contrato — 0.15.8

Savev2 e seis IDs de talentos preservados. Protocolo de sessão11: acrescenta intenção ClaimAllDeliveries, validada pelo host via ProfileService. Recebimento em lote move instâncias até a capacidade da mochila numa única transação; excedentes permanecem em Entregas. A interface lista o catálogo de talentos com rolagem e metadados visuais opcionais; expansão de IDs/ranks requer futura migração de dados, sem simulação de habilidades. Detalhes atuais: [entregas e talentos](ENTREGAS_E_TALENTOS_0_15_8.md). Seções abaixo mantêm o histórico anterior.

---

# Atualização 0.15.4 — apresentação modular

MenuArtwork usa duas texturas Resources originais com recorte proporcional sem raycast. CampaignHome/CampaignHud preservam as ações reais de CoopCloud/CoopDirector. CampaignHeroPicker consulta HeroRoster.At e usa SelectHero; não instancia atores adicionais. Solo aguarda Lobby e conexão concluída antes de StartGame. Mensagens de erro são traduzidas para o jogador, com diagnóstico completo no log; nenhum serviço novo foi criado.

PresentationQuality pertence ao objeto da interface da cena, mantém cópia runtime do URP Asset e restaura a referência na destruição. PC mantém Deferred+SMAA/FXAA; celular mantém Forward+MSAA. Bordas de RoundedHudGraphic e HudIcon têm cobertura alfa de um pixel físico para Canvas overlay, que não depende da câmera. Savev2 e protocolo10 preservados. [Verificações e limitações](APRESENTACAO_0_15_4.md).

---

# Atualização 0.15.3 — apresentação do inventário

CoopInventoryHud apresenta as listas existentes sem reordenar dados persistidos. ItemDescriptions centraliza resumo, restrições e comparação usando ProgressionStats.Calculate, separando ficha do herói de comparação item a item. InventoryRules.EquippedBy consulta a propriedade de slots; a autoridade de transações permanece no ProfileService/CoopProgression. Entregas só são exibidas como recebidas após a propriedade aparecer no perfil. CampaignProfileHub distingue perfil deste aparelho de cópia da sala, sem criar conta global. Protocolo10, savev2 e fórmulas preservados. [Testes e limites](INVENTARIO_0_15_3.md).

---

# Atualização 0.15.2 — direção consistente de combate

IWorldAimCommands fornece uma direção de mundo opcional. ValidatedHeroCommands conserva apenas a última mira finita/normalizada e válida por sequência/timeout. HeroCombat captura essa intenção no começo da ação e mantém a orientação de conjurações com projétil até o fim da ação, sem interromper a locomoção. CombatProjectile alinha transform.forward ao voo ao reutilizar o pool; a replicação visual existente usa esse mesmo transform. Não foram acrescentados bytes aos comandos nem mudados protocolo10/saves v2.

AimTargetSelector faz varredura sem alocação por ciclo sobre os Health já existentes do pool de inimigos. Retenção exige alvo vivo, ativo, presente, dentro do alcance e com visão livre. Um candidato precisa superar a razão/distância mínima configuradas no CoopDirector; alvo inválido é liberado imediatamente na próxima varredura. Mouse permanece manual. [Validação e limites](MIRA_0_15_2.md).

---

# Atualização 0.14 — arsenal, efeitos e exploração

O arsenal0.13 acrescentou nove malhas/ícones originais e12 clipes reais para a Brasa. MeleeGeometry centraliza cone/linha/círculo usados pelo acerto e indicador. ProgressionStats mostra dano numérico, defesa percentual limitada, chance crítica e multiplicador crítico; ItemStat.CriticalDamage foi acrescentado ao final do enum para preservar saves.

CombatBuildEffects aplica perks do equipamento, sinergias limitadas, escolhas temporárias de altares e contra-ataque da esquiva perfeita. CombatAilments guarda estados da vítima, limpa no reaproveitamento e só simula dano na autoridade. ActorImpactFeedback usa sons originais, rastro compartilhado e pausa na ação de HeroModelView sem alterar Time.timeScale ou congelar locomoção. CombatAudioPool limita os novos sons a oito vozes; avisos existentes têm pool separado. Configurações permitem volume de efeitos e impacto de câmera/vibração opcionais.

CampaignFeatures estende a masmorra existente com dois altares, esconderijos, desafio opcional por bioma, barris, parede quebrável e ponte frágil. Usa dados ExpeditionFeatureDefinition e NavMesh existente, sem substituir geração ou encontros. SideActivityBusy impede iniciar encontro principal durante o desafio. Waves/Defend/Escort usam EnemyPool com orçamento finito e objetivo auxiliar; sucesso não conclui sala principal. Recompensas usam CoopRewardLedger e regras do catálogo com IDs separados. Pontes expõem uma área de perigo sobre o chão existente, sem simulação de queda. Arte é modular provisória original.

CampaignAdventureChannel aceita apenas intenções limitadas por conexão; o host valida identidade, fase, vida, proximidade e visão. O cliente não escolhe dano, recompensa nem coordenadas finais de marcação. CampaignRun snapshot transporta nós/pings; ActorFrame transporta bênçãos, contra-ataque e estados visuais. Protocolo10 exige versão0.14 nos participantes. Não foram adicionados pacotes. Perfil/save permanece versão2, com os limites de confiança no host já documentados.

EquipmentPaperDoll continua sendo apenas prévia visual isolada; os slots usam ícones reais e silhuetas quando vazios. AdventurePolishInstaller.Upgrade migra somente os dados desta revisão. CampaignInstaller gera saídas estáveis Builds/Atual/Windows e Builds/Atual/Android; mesmo pacote Android com.forjadasmares.campaign, sem instalar outra aplicação a cada build.

Evidências atuais e limites: REVISAO_0_14_2026-10-20.md. Os blocos abaixo são históricos.

---

# Atualização 0.12 — propriedade, talentos e economia

InventoryRules centraliza posse visual, capacidade livre da mochila e valor de reciclagem. A coleção persistente conserva IDs únicos de itens possuídos; loadouts referenciam esses IDs. A interface exclui todos os IDs equipados da mochila/saque, sem copiar ou apagar o item. A mesma instância não pode equipar dois heróis. Desequipar com mochila cheia falha sem perder o equipamento.

HeroTalents separa seis regras configuráveis no ProgressionCatalog das alocações persistidas em PlayerProfile. Cada herói dispõe de nível global − 1 pontos; máximo cinco graduações por melhoria. SpendTalent/ResetTalents são transações persistidas e intenções validadas pelo host na rede. HeroEquipment inclui o conjunto de talentos na assinatura e aplica ProgressionStats às funções reais. Equipamentos e talentos compartilham os limites existentes; sem crescimento multiplicativo infinito.

RewardRule.instantCurrency credita XP/ouro na transação da morte. ExpeditionData.bankedExperience/bankedCoins registra a parcela já paga: finalizar, morrer ou retomar após interrupção não paga novamente. Saves versão 2 anteriores têm esses campos em zero e seguem a consolidação anterior. CoopRewardLedger mantém idempotência por evento e participante. A campanha não concede mais item/ouro por RoomWon; mantém reviver aliados. Baús, chefe, exploração e conclusão têm tabelas próprias.

CampaignLootDrops recebe mensagens confiáveis somente do host após salvar a recompensa; não existe intenção do cliente para conceder ouro. Seus 32 tokens geométricos são apresentação temporária, com recolhimento automático. Cada participante recebe seus próprios prêmios, sem disputa. Equipamentos recebidos ficam no SAQUE ATUAL para uso fora de combate e são consolidados no fim; excesso usa as regras de capacidade existentes. Invocações recompensam pouco e mantêm orçamento finito.

CampaignCharacterSheet, ItemDescriptions e o painel ItemComparison reutilizam CoopProfileChannel; sem mutações diretas da UI. Comparação mostra bônus intrínsecos e diferença, depois a prévia geral. A última sala em cache permite apenas o planejamento de equipamento existente; investir talentos/reciclar exige o host de origem ou o perfil deste aparelho. Ouro pode pagar redistribuição de talentos (25 por ponto investido).

HUD: minimapa compacto superior esquerdo, vida/retrato ao lado e objetivo resumido. EquipmentPaperDoll usa mundos de prévia separados; retrato de 128×128 captura apenas quando muda o visual. Não cria outro ator de combate. Protocolo 8: cliente 0.11 não entra numa sala 0.12. Nenhuma dependência adicionada.

---

# Atualização implementada — 0.11.0

CampaignForgeInstaller migra a CampaignCoop existente para dados próprios de campanha (4 heróis, 23 definições de equipamento), preservando IDs e cenas de referência. HeroCombat recebe combos e habilidade da arma via HeroEquipment. Ataques focados têm maximumTargets=1; StrikeOrigin/OriginClear impedem iniciar golpes/projéteis além de cobertura. Fúria é estado temporário do combate, replicado por ActorFrame; não aplica dano direto, não acumula nem recarrega a barra enquanto ativa. Protocolo 7 exige todos os clientes atualizados.

CampaignProfileHub e CoopProfileChannel permitem usar o perfil no início sem sala. ESTE APARELHO é o mesmo arquivo do participante quando ele hospeda. ÚLTIMA SALA é um cache confirmado pelo host de origem; só intenções limitadas de equipar/desequipar retornam ao mesmo host no lobby, com validação de posse, classe, slot, nível e estado. Não existe conta global, transferência confiável entre hosts, servidor dedicado ou migração de host. O host ainda pode adulterar sua própria autoridade.

ProfileStore mantém versão 2/checksum/backup. A migração Forge retira dos slots itens antigos que se tornaram incompatíveis, conservando-os na mochila junto com XP/moedas. Arquivos ilegíveis são preservados antes de criar substituto. Marcadores por classe tornam armas iniciais e a primeira arma obtida não repetíveis. A primeira vitória concede uma arma comum compatível e utilizável no nível 1; nunca equipa automaticamente. Novas concessões continuam no ledger da autoridade.

CampaignMinimap desenha salas/corredores reais, aliados, inimigos, objetivos e encontro pendente; atualiza a 5 Hz. Snapshot acrescenta fightRoom e aviso de chegada de inimigos. Ondas intermediárias podem reforçar com até dois remanescentes; conclusão final exige todos. Emissores e anéis de chegada são geometria provisória. CampaignAbilityBar liga os seis botões a ações reais, com ícones, cooldowns e atalhos; joystick/toque existentes preservados.

Dependências de rede/renderização/entrada mantidas. Capturas locais de diagnóstico exigem o módulo builtin com.unity.modules.screencapture 1.0.0 (imageconversion 1.0.0). Comandos do CoopSmokeProbe só existem em desenvolvimento/editor e dependem de diretório local -netProbe; não são pedidos de rede. VS Code permanece o editor.

---

# Arquitetura implementada — atualização 0.9.1

A revisão reutiliza CampaignRun, DungeonGenerator, EncounterDirector, HeroCombat, Health, CoopDirector e os perfis existentes. CampaignAdventureDefinition fornece os dados; CampaignAdventure cria props determinísticos dentro do NavMesh, com máximo48; CampaignAdventureProp usa Health/IHitDefense. Layer14 CampaignProps recebe dano sem bloquear jogadores/agentes. SnapshotAdventure replica vida, fontes usadas e recompensa concluída.

O host valida intenção, alcance/obstáculos e dano, controla RNG/recompensas e persiste via CoopProgression/CoopRewardLedger. Cliente não fornece valor final de dano, carga ou recompensa. Eventos adventure:id são idempotentes. IsRewardRule centraliza as tabelas autorizadas no catálogo. Save versão existente preservado; protocolo de rede5 exige clientes atualizados.

CampaignCombatReadout observa mudanças reais de Health e perfis, com pool20 e liberação de eventos. CampaignSessionMenu usa InputAction para Esc/Start e mapa baseado em salas reais. WarriorOnlineFlow é dado por herói: ataque carregado/SUPER reaproveitam HeroCombat e clips reais de Brasa; não são poderes universais fictícios.

A autoridade em host não protege contra host adulterado. Sem migração de host/servidor dedicado. Dependências e resultados detalhados em `AVENTURA_0_9_2026-10-17.md`. O plano histórico abaixo permanece como referência; recursos planejados não equivalem a implementados.

---

# Arquitetura proposta — Forja das Marés

Estado: documento mestre iniciado na etapa 1. As áreas futuras abaixo são planejamento. A arena local da etapa 2 já foi implementada; o catálogo real e a validação estão em PROGRESSO.md.

Atualização para a etapa 2: alvos atuais **Windows e Android**. iOS está adiado por instrução posterior do usuário. As seções de longo prazo permanecem como planejamento, não como recursos implementados. A arena usa o template oficial Universal 3D e componentes de entrada, movimento, mira, câmera, dano, feedback e HUD. Consultar `PROGRESSO.md` para a lista real de arquivos e evidências de validação.

## Produto e primeiro recorte

Cooperativo de ação e RPG, câmera diagonal de cima, até quatro jogadores, Windows/Android (iOS adiado). Sessões privadas curtas são o primeiro objetivo online. Meta de design inicial: expedições de 8 a 12 minutos, a ajustar com sessões reais. A campanha comercial completa será expandida em entregas incrementais.

Primeiro recorte local: arena legível, um herói de teste com placeholder claramente identificado, movimento relativo à câmera, ataque próximo, esquiva e alvo de treino. Nenhuma animação final ou modelo será declarado pronto a partir de uma primitiva.

## Organização proposta

| Área futura | Responsabilidade | Dependências permitidas |
| --- | --- | --- |
| Core | Identificadores, resultados de ações, relógio de simulação e contratos pequenos | C# e dependências mínimas |
| Data | ScriptableObjects e validação de conteúdo | Core e Unity |
| Gameplay | Movimento, combate, habilidades, atributos, IA, salas, drops e progressão | Core, Data e Unity; sem SDK de serviços |
| Presentation | Modelos, câmera, animação, VFX e áudio a partir do estado do jogo | Gameplay/Core; sem calcular dano ou conceder loot |
| Input | Adaptadores do Input System que produzem comandos | Core e Input System |
| UI | HUD, menus e apresentação de inventário/opções | Dados de leitura e ações explícitas; sem aplicar recompensas |
| Persistence | Serialização, migração e gravação do perfil | Core e dados persistentes; sem objetos de cena |
| Networking | NGO/Transport, replicação, validação de pedidos, sessão e UGS | Core/Gameplay e SDKs de rede |
| Bootstrap | Inicialização e composição das dependências da cena | Módulos necessários |
| Editor | Ferramentas e validações usadas apenas no Editor | Código de runtime quando necessário |

Usar `Assets/ForjaDasMares/` como raiz futura do conteúdo próprio e `Docs/` para decisões/progresso. Criar pastas e assembly definitions apenas quando houver código a isolar. Não gerar dezenas de componentes vazios. Assemblies de runtime nunca devem depender de UnityEditor; a camada Gameplay não deve conhecer Lobby/Relay.

Composição por referências serializadas ou injeção explícita na inicialização. Evitar um gerenciador global para todos os sistemas, buscas repetidas na cena e um barramento universal que esconda dependências. Eventos tipados locais podem comunicar alterações de vida, ataque e conclusão de sala; o estado autoritativo permanece consultável.

## Dados e estado

ScriptableObjects armazenam configurações compartilhadas e imutáveis em execução. Cada conteúdo tem identificador estável próprio, independente do nome de arquivo. Estado de uma instância fica em objetos/runtime separados; não gravar vida, cooldown, inventário ou XP de um jogador no asset compartilhado.

| Definição futura | Dados principais |
| --- | --- |
| Herói | ID, atributos base, movimento, habilidade inicial e referências de apresentação |
| Arma | ID, categoria, alcance, dano, custo, passos de combo e habilidades compatíveis |
| Habilidade | ID, cooldown, custo, execução, alcance, alvos, interrupções e apresentação |
| Inimigo | ID, atributos, percepção, comportamento e tabela de recompensas |
| Sala | ID, sockets de conexão, limites, encontros, objetivos e restrições de geração |
| Equipamento/raridade | ID, peso de sorteio, modificadores permitidos e limites |
| Missão/dificuldade | Objetivos e modificadores explícitos de encontro/recompensa |

Instâncias de equipamento têm ID único, definição, raridade e modificadores sorteados. Definições e saves têm versão de esquema. O catálogo valida IDs duplicados, referências ausentes, custos negativos e salas sem conexão. Fazer esse validador quando o primeiro catálogo real existir.

## Movimento, câmera e combate

Plano de jogo XZ, gravidade no eixo Y. Começar com CharacterController para movimento responsivo em terreno simples; confirmar essa escolha ao inspecionar a futura cena. Separar leitura de input, simulação de deslocamento e apresentação. A câmera diagonal acompanha o alvo suavemente, sem controlar as regras de movimento.

Um comando de jogador representa direção, mira e intenções de ataque/esquiva/habilidade. Teclado, mouse, gamepad e toque produzem o mesmo formato. Não usar UI como fonte da verdade nem enviar posição final livremente para o host.

Combate baseado em estados com preparação, janela ativa e recuperação. O relógio da simulação determina as janelas; animação e VFX acompanham o resultado. Não depender de Animation Events para decidir dano. Um ataque possui ID e registra os alvos atingidos para evitar dano repetido por vários colliders.

Ataque próximo usa consultas físicas limitadas e buffers reutilizados quando adequado. Não exige criar um projétil por golpe. Esquiva define duração, custo, deslocamento e janela de invulnerabilidade explícita. Cancelamentos e encadeamento dos combos precisam de regras pequenas e observáveis; balanceamento começa com medidas de resposta e clareza.

O resolvedor de dano verifica autoria, alvo vivo, distância, janela, equipe, invulnerabilidade e modificadores. Só ele altera a vida. UI e apresentação recebem o resultado. A arena offline usa o mesmo caminho de resolução que posteriormente será executado pelo host.

## Heróis e conteúdo original

Ideias provisórias para a etapa 3: Guardião da Âncora (próximo e defesa), Tecelã da Maré (controle de área) e Batedor de Cobre (mobilidade e distância). São conceitos, sem assets ou habilidades implementadas. A seleção final será determinada por contraste de silhueta, controles em toque e variedade no grupo.

Introduzir um herói completo primeiro, depois outro estilo. Compartilhar atributos, estados de ação, equipamentos e interfaces de habilidade. Evitar copiar um controlador inteiro por herói.

## IA, masmorras e geração

IA inicialmente com estados pequenos: inativo, perceber, aproximar, preparar ataque, executar, recuperar, morrer. Escalonar percepção e decisões de hordas para evitar consultas caras em todos os inimigos a cada frame. Navegação pode usar AI Navigation/NavMesh, após verificar o pacote e a geometria na etapa 4. IA e criação de encontros pertencem ao host quando a rede existir.

Geração controlada usa salas próprias previamente validadas e um grafo conectado. Separar topologia, montagem espacial e escolha de encontros. Garantir entrada, saída, caminho até o chefe, sockets válidos e ausência de sobreposição; limitar tentativas e ter uma rota de fallback pré-validada.

O host decide seed, versão do algoritmo e conteúdo selecionado. Seed sozinha não garante que clientes com catálogos/versões diferentes produzam a mesma masmorra: enviar e validar IDs e conexões definitivas. Não depender de simulação física determinística entre plataformas.

Armadilhas, baús e portas possuem estado autoritativo e interações identificadas. Salas comunicam conclusão por objetivos reais, não apenas por uma chamada da interface.

## Progressão, inventário e persistência

Primeiro inventário simples com capacidade e regras de equipamento explícitas. XP, moedas, equipamentos, melhorias, mascotes e missões entram depois do combate/masmorra funcionarem. Separar progresso da expedição do perfil persistente. O mascote inicial deve ter uma função limitada e clara; não criar subsistemas de criação/comércio antecipadamente.

Resultados usam ID de expedição e ID de concessão para evitar duplicações em retries. O host sorteia loot e concede moedas/XP; clientes pedem coleta, equipar ou abrir baús. O host valida ownership, capacidade, distância e estado antes de aplicar. Inventário e recompensas não devem ser aceitos de mensagens do cliente sem validação.

Save local em `Application.persistentDataPath`, versão de esquema, gravação temporária seguida de substituição e backup anterior. Validar dados carregados e migrar versões explicitamente. Não serializar MonoBehaviours nem referências vivas da cena.

Perfil local e host de jogador servem às sessões privadas iniciais, mas não oferecem economia comercial resistente a adulteração. Para economia confiável e perfil entre dispositivos será necessário backend com identidade persistente e regras de concessão verificadas; crossplay de partida e sincronização de perfil são entregas diferentes.

## Rede e autoridade

Proposta: NGO + Unity Transport + Multiplayer Services SDK (Sessions/Lobby/Relay) + Authentication. Fixar versões somente após confirmar a linha do Editor, resolver dependências e testar na etapa 7. Não usar autoridade distribuída nesta proposta. Relay transporta dados; não executa simulação nem valida recompensas.

| Ação/estado | Decisão no host | Cliente |
| --- | --- | --- |
| Movimento | Validar comandos, velocidade, colisão, tick e estado de ação | Enviar intenção e apresentar estado recebido |
| Ataque/habilidade | Verificar custo, cooldown, alvos e janela; aplicar resultado | Pedir execução e mostrar feedback |
| IA/chefe/armadilha | Simular decisões, dano e mudanças de fase | Apresentar estados/eventos |
| Baú/loot/equipamento | Sortear, conceder, validar coleta e ownership | Pedir interação |
| Sala/seed/resultado | Selecionar conteúdo, concluir objetivos e emitir resultado único | Receber configuração e resultado |

Pedidos carregam IDs e sequências, têm limites de frequência e rejeitam conteúdo inválido. Verificar no host o jogador associado à conexão, sem confiar no ID declarado pelo remetente. Validar versão de protocolo e catálogo no ingresso. Nunca aceitar dano, loot ou conclusão de sala calculados exclusivamente pelo cliente.

O primeiro multiplayer usa movimentação autoritativa com interpolação. Se a latência causar resposta insatisfatória, adicionar previsão local e reconciliação limitada de movimento, medida com latência real. Isso é trabalho explícito, não uma capacidade já pronta do transporte. Evitar previsão de loot/resultados.

Host local é uma escolha de custo e complexidade para pequenos grupos privados. O dono do host ainda pode adulterar a simulação: autoridade do host não equivale a segurança de servidor dedicado. Deixar composição da simulação preparada para um processo de servidor futuramente, sem implementar dois ambientes nesta etapa.

Salas privadas terão até quatro jogadores e código de entrada; convites nativos de plataforma são entrega separada. Reconexão requer identidade persistente, reserva temporária do slot, snapshot de estado, sincronização de inventário e política de expiração. Planejar pausa/retorno de app móvel. Inicialmente, queda do host encerra a partida com resultado definido; reconectar um cliente não resolve migração do host. Migração transparente deve ser planejada e testada separadamente.

## Apresentação, UI e acessibilidade

URP com orçamento voltado ao aparelho mínimo definido antes da otimização final. Silhuetas e telegráficos legíveis, iluminação moderada e controle de sombras/partículas. Arte de placeholder própria deve ser identificada e substituível sem mudar regras do combate.

HUD mostra vida, recursos, esquiva e habilidades. Input System separa mapas Gameplay e UI. Toque usa joystick e botões com mira/assistência ajustáveis; teclado/mouse e gamepad recebem ações equivalentes. Prever safe area, escalonamento, remapeamento, tamanho de texto, intensidade de tremor, flashes e pistas de forma/ícone além de cor. Definir orientação móvel paisagem como proposta inicial.

Áudio reage a eventos confirmados; volume de música/efeitos separado. Não afirmar existência de trilhas, efeitos ou animações antes de incorporá-los ao projeto.

## Pooling e desempenho

Pool por prefab para projéteis, VFX e objetos frequentes; criar quando usados. Resetar vida, timers, assinaturas, referências e estado de colisão ao devolver/alugar. Em rede, alinhar spawn/despawn com ciclo de vida de NetworkObjects; um pool local sozinho não sincroniza objetos.

Medir CPU/GPU, alocações, draw calls e tráfego em aparelhos reais. Orçamento inicial de 60 FPS no PC e opção de 30/60 FPS móvel é uma meta, sem garantia de desempenho. Definir dispositivo mínimo, máximo de inimigos e partículas com profiling do primeiro recorte, antes de prometer hordas numericamente.

## Critérios de validação futuros

- Regras: cooldown, golpe único por alvo, atributos, geração conectada, concessão idempotente e migração de save.
- Integração no Editor: colisões, ataque/esquiva, transição de salas e reentrada de objetos no pool.
- Rede: 2 a 4 jogadores, atraso/perda, comandos inválidos, duplicações, ingresso tardio, queda do host e reconexão do cliente.
- Crossplay real: Windows + Android + iPhone/iPad na mesma sessão, mesma versão de protocolo/catálogo, background/retorno e redes Wi-Fi/celular.
- Builds: Windows, Android com toolchain do Editor e iOS em ambiente macOS/Xcode ou serviço de build apropriado; assinatura e distribuição são configurações futuras.

Um teste local com dois clientes Windows não demonstra crossplay. Registrar aparelhos, versões, cenários executados e falhas. Nenhum desses testes foi executado nesta etapa.

# Atualização da etapa 3 — heróis locais

HeroDefinition referencia AttackDefinition[] e AbilityDefinition, separando tuning do comportamento. HeroRoster seleciona a configuração e a silhueta; HeroCombat controla preparação, recuperação, cadeia, habilidade e cooldown. DamageResolver realiza alcance/arco, linha de visão, facção e deduplicação por Health. CombatProjectile faz spherecast contínuo para evitar atravessar obstáculos; CombatZone resolve armadilhas de duração finita. CombatEffectPool cria a capacidade uma vez e reutiliza instâncias. Health contém vida, armadura, proteção e controle temporário; PlayerMotor limita a janela de invulnerabilidade e conserva colisões.

HeroPresentation possui estados visuais procedurais; não há clips finais. HeroHud mostra vida/cooldown e a arena tem controles de teste. IHeroCommands estende a interface de entrada existente. TouchMoveStick usa eventos do EventSystem/Input System para um vetor capturado por dedo com base imóvel, enviado a ArenaInput. O resto dos botões usa controles virtuais. A cena mantém câmera, chão, iluminação, obstáculos e spawn existentes. O ataque anterior fica desabilitado, preservado para regressão.

Autoridade de servidor continua planejada, sem implementação nesta etapa. Vida, ataques, posição e efeitos são locais; não considerar a API de simulação pública uma validação de rede. Pool e resolver têm capacidades finitas expostas/documentadas; profiling real e testes em vários aparelhos permanecem pendentes. Sem equipamento/progressão/multiplayer.

# Estado atual14/10/2026 — etapas locais4–6 e cooperativo7

As seções anteriores registram checkpoints históricos. O estado atual inclui PvE, mapas, progressão e uma sessão cooperativa privada LAN integrada; internet continua pendente. Detalhamento dos módulos locais em PVE_ARQUITETURA.md, MASMORRAS_ARQUITETURA.md e PROGRESSAO_ARQUITETURA.md; migração/versões/configuração em REDE_PLANO_MIGRACAO.md e REDE_PROGRESSAO.md.

- Lógica local preservada: EnemyBrain/EnemyCombat/EncounterController simulam IA/ondas/chefe; DungeonLayout/Generator/Session controlam seed/conexões/objetivos; ProfileService/Store e ProgressionStats calculam/validam/gravam inventário e recompensas. CombatParty fornece seleção de jogadores vivos.
- CoopDirector compõe quatro CoopActor e valida comandos via ValidatedHeroCommands. Host executa os componentes existentes; cliente desativa movimento/física/combate autoritativos e interpola poses/vida/estado recebidos. CoopReplicaView reutiliza inimigos e efeitos como apresentação. NGO CustomMessagingManager/UnityTransport levam intents/snapshots limitados, com aprovação/limites/confirmacão de layout. Não há predição avançada nem migração do host.
- CoopProgression liga os eventos reais ao CoopRewardLedger e aos perfis privados do host. CoopProfileChannel entrega inventários somente aos respectivos participantes e valida intents de equipamento/descarte. CoopInventoryHud consulta o espelho e bloqueia apenas entrada local. Armas alteram os ataques da autoridade e sua identificação visual é replicada. Health.Revive explicita reaparecimento após vitória da sala; Heal permanece incapaz de ressuscitar.
- Persistência offline e cooperativa ficam separadas. Perfil do cliente não é importado como autoridade. GUID persistente/PIN são suficientes para protótipo privado, não autenticação/economia comercial. Host alterado pode adulterar seus próprios resultados; backend autenticado é dependência futura.
- Composição cooperativa está em CoopDungeon; variantes ProgressionDungeon/ProgressionArena continuam locais. Referências obrigatórias são configuradas pelo instalador; ArenaTest/DungeonTest permanecem preservadas. CoopHud é uma interface mínima, distinta do escopo da etapa completa de UI ainda pendente.

Evidência: regressão89/89, rede17/17 repetida nas últimas correções, quatro processos Windows com recompensa/reload e PC–moto g15 real com movimento/ataque/equipagem/reconexão/resultado/reaparecimento. Ver ANDROID_TESTE_FISICO_2026-10-14.md para distinguir controles físicos, bot de combate e mortes administrativas de QA. Não existe validação Relay/internet, quatro celulares ou lançamento. Builds finais0.6.1 Windows/Android foram produzidas; UI/arte final/profiling/expansão permanecem na fila autorizada.

## Continuação15/10 — internet privada

Vínculo Unity Cloud real configurado; builds Windows/Android0.6.2 conectadas pelo MPS/Lobby/Relay com combate, movimento, recompensa e retorno após encerrar processo, inclusive Android físico em Wi-Fi. CoopInternetSettings centraliza ambiente/região/timeout; CoopCloud persiste somente referência local de sessão por perfil SDK para usar ReconnectToSessionAsync quando o Lobby ainda mantém associação. Cache não autentica nem concede economia. Host continua simulação/persistência autoritativa do protótipo, sem migração ou servidor dedicado. Ler REDE_INTERNET_2026-10-15.md e INTERNET_E_PUBLICACAO.md para evidências, publicação e pontos reais de migração.21 testes específicos:20 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. A ressalva histórica de ausência de Relay acima foi superada apenas neste recorte; redes distintas/quatro clientes Relay/lançamento seguem sem validação.


## Campanha0.8.1 — 17/10/2026

Campanha permanece uma variante da base cooperativa: CampaignRun estende dados/salas e observa a Party; CoopDirector é a autoridade e CoopCloud gerencia descoberta/Relay. Não existe dependência do assembly de gameplay para networking. Solo é modificador transitório em HeroEquipment, calculado por ProgressionStats.WithSoloBonus; vida mantém proporção para não curar ao mudar o grupo. Placas alternam entre ativação sequencial no solo e simultânea em grupo.

Novas entradas: mesma fase/protocolo4, antes de20% de setores concluídos, máximo4 participantes/reservas da tentativa. Envio inicial do mapa e perfil é confirmado/repetido sem confiar em valores do cliente. Vida/movimento só são habilitados no host após o cliente confirmar o mapa; ponto de chegada é reamostrado no setor atual. Perfil novo entra no ledger para eventos futuros. Uma identidade não pode ter dois escritores ativos no mesmo save; participantes do ledger atual preservam a vaga durante a tentativa, mesmo depois de90s. Conclusões/recompensas antigas não são reproduzidas ao recém-chegado.

CampaignHome oferece tela inicial e agenda local de códigos de convite, sem fabricar presença social. Amigos autenticados, identidade global e economia segura por servidor dedicado continuam pendentes. Alterações e evidências específicas em CAMPANHA_SOLO_2026-10-17.md; uso em CAMPANHA_COMO_JOGAR.md. Sem novos pacotes. Builds Windows/Android de desenvolvimento, sem aprovação comercial.

## Menu de sessão da campanha 0.8.2
CampaignHud cria CampaignSessionMenu na SafeRoot existente. Overlay bloqueia somente ArenaInput local e coordena abertura exclusiva com CoopInventoryHud, sem alterar o relógio da simulação. Visibilidade usa CoopCloud real; nenhuma mensagem de rede nova/protocolo alterado. Volume/controle visual persistidos em PlayerPrefs no aparelho; convites consultam a agenda CampaignInvitations-v1 existente. Sem rede social/presença/WhatsApp automático.

## Revisão0.10.1 — campanha aberta, hordas e equipamento visual

CampaignLevel define openLandscape, sectorSize e passageWidth; DungeonLayout mantém espaçamento por instância, sem alterar o layout legado. DungeonSession conserva activeEncounterRoom separadamente do setor visitado e usa essa referência para conclusão. Portas não fecham nessa campanha. Encontros seguem serializados, enquanto objetivos/interações localizam o setor físico de cada ator.

Campanha usa EnemyDefinitions/prefabs clonados, preservando arena/masmorras anteriores. EnemyPool respeita a capacidade por arquétipo; EncounterController mantém PlannedCount/DefeatedCount sincronizados no CampaignSnapshot/protocolo6. Máximo simultâneo definido pela plataforma do host20Android/24Windows, não100 simultâneos. EnemyModelMotion é apresentação procedural, sem decidir dano.

EquipmentDefinition contém ícone de categoria e sequência de ataques. Profile.Item resolve banco+expedição sem unir as coleções; transações preservam IDs, regras de equipagem e finalização única. CoopProgression valida equipagem em campanha aberta sem perigos próximos. CoopProfileChannel omite o diário de eventos interno do espelho, preservando o diário no host. EquipmentVisualPlacement compensa lossyScale do pai ao montar arma nos ossos importados e réplicas. Não altera o collider ou os atributos do herói.

CameraOcclusion guarda conjuntos de renderers filhos por collider e aplica material parcialmente transparente sem desligar colisão. CampaignCombatReadout apresenta sons e números por deltas de estado, com pools/frequência limitada. Fontes de arte editáveis e galeria: ArtSource/ExpeditionSet. DependênciasUnity/pacotes/serviços permanecem nas versões anteriores; sem novos serviços pagos ou segredos. Relatório e limitações: PAISAGENS_E_HORDAS_0_10_2026-10-17.md.


## Correção0.14.1: chão e interface
ActorMotion subdivide frames longos (passo alvo0,05s, máximo32). PlayerMotor guarda piso de layer configurável e resgata queda acidental após3m, preservando vida e cooldown e sem reviver; autoridade apenas. Mapas verticais/queda intencional exigem configuração e novo teste. FallRecoveries é telemetria local do diagnóstico, não campo do protocolo10. CampaignAdventureHud posiciona quatro rótulos em candidatos sem sobreposição dentro da interface; CoopSmokeProbe usa CoopDirector.LocalInput explícito. Sem mudanças de saves ou dependências. Evidências em Evidencias_0_14_1.

No layout móvel, QuickPingPosition separa a barra de marcações do joystick padrão; os quatro botões têm largura suficiente para os textos. Teste físico PC–moto g15 no mesmo Wi-Fi concluído na0.14.1, incluindo toque, reconexão e recompensas. Relatório: REVISAO_0_14_1_2026-10-21.md.


## HUD0.14.2
Removida a construção do painel QuickPings e seus quatro botões. Sem área invisível de interação; G/direcional para cima e a apresentação de marcações opcionais continuam usando o canal existente. Sem mudança do protocolo10 ou saves.

## Prisma 0.15.0 — apresentação do Arcanista

Prisma usa HeroModelView/Animator genérico com 25 clipes e máscara de tronco; o mesmo BrasaFootPlant é reutilizado para pés. As oito cenas mantêm as raízes HeroVisual_arcanist e recebem uma única instância do prefab autoral. HeroPresentation desliga as partes geométricas antigas e troca também o CombatFeelDefinition quando muda a classe. EquipmentPaperDoll copia somente a apresentação, desabilitando linhas e scripts para não criar outro ator funcional.

HeroModelView resolve animações de cajado, grimório e orbe com fallback para os estados genéricos. EquipmentDefinition tem customGrip/socketPosition/socketEuler opcionais; EquipmentVisualPlacement aplica o mesmo encaixe e compensação de escala na autoridade e nas réplicas. Itens antigos sem customGrip preservam o encaixe anterior. É apresentação, sem alteração de colisores ou atributos.

PrismaCastingView lê HeroPresentation (incluindo carga replicada), com oito linhas e arrays reutilizados. PooledProjectileTrail permanece vinculado aos slots finitos do pool/proxies. Efeitos não concedem dano. HeroAttackArea usa os dados do super quando variante4; os golpes continuam resolvidos uma única vez pelos sistemas existentes. Foco de Brasa conserva sua explosão e cadência específicas.

CoopDirector envia HeroPresentation.ActionProgress, que seleciona o relógio de ataque, esquiva ou reação a dano. O campo animation já existia: protocolo10 e salvamento v2 não mudam. Dados de dano/controle, RNG, inventário e recompensas continuam na autoridade. Sem novo serviço/pacote; teste desta revisão em Android físico continua necessário. Fontes, valores e evidências: PRISMA_0_15_0.md.
