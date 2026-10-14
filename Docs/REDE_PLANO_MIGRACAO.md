# Migração cooperativa — auditoria e plano da etapa7

Ponto de partida: etapa6 local0.5.0,68 testes gerais e9 testes de revisão de UI aprovados; Windows observado e persistência em reinício real validada; APK Android gerado, sem aparelho conectado. Código existente será reutilizado. Ainda não há rede implementada neste plano.

## Auditoria e adaptações

| Sistema atual | Reutilizar | Adaptação necessária |
|---|---|---|
| ArenaInput/IHeroCommands/PlayerMotor | Intenções, movimento normalizado, colisões, esquiva | Comandos por jogador com sequência/limites, relógio do host, interpolação/reconciliação; cliente não envia posição final. |
| HeroCombat/Health/DamageResolver/pools | Ataques, cooldowns, deduplicação, facções e dados | Simulação de dano somente no host; estados/efeitos visuais no cliente; snapshots de atributos e validação de pedidos. |
| EnemyPool/EnemyBrain/EnemyCombat | Prefabs, navmesh, percepção, ataques e chefes | Seleção entre jogadores vivos, limite de equipe, uma simulação de IA no host; proxies visuais nos clientes. |
| DungeonLayout/Generator/Session | Seed/grafo, salas, portas e objetivos | Layout/hash compartilhados, carregamento confirmado, transição de sala coordenada e estado autoritativo de portas/objetos. |
| ProfileService/LootGenerator/Store | Transações, ledger e dados | RNG/loot da partida no host; arquivos locais não são prova de ownership/atributos. Cooperação privada usa regras controladas pela sessão; economia comercial exige backend confiável. |
| HUD/inventário | Leitura de estado e ações funcionais | Menu de sessão e jogadores, estado local/remoto, confirmação de saída, proibição de ações incoerentes durante carregamento. |

## Dependências verificadas

Consulta ao registro oficial packages.unity.com em14/10/2026:

- NGO mais recente no registro:3.1.0, exige Unity6000.7; não é adequado à6000.3.26f1 atual. Linha estável compatível encontrada: **com.unity.netcode.gameobjects2.13.3**, mínimo6000.0, dependência Transport2.6.0.
- **com.unity.transport2.7.4**, mínimo2022.3.
- **com.unity.services.multiplayer2.4.0**, mínimo6000.0, exige Transport2.7.4, Collections2.6.8, Core1.18.0 e Authentication3.8.0, além de QOS/Wire/Newtonsoft/UnityWebRequest.
- Preferir MPS unificado para Sessions/Lobby/Relay, conforme [instalação oficial](https://docs.unity.com/en-us/mps-sdk/install-and-upgrade), [sessões](https://docs.unity.com/en-us/mps-sdk/sessions-overview) e [Relay](https://docs.unity.com/en-us/mps-sdk/networking/relay-integration). Arquivos oficiais foram baixados fora do projeto para inspeção das APIs. Versões ainda precisam resolver/importar/compilar nesta Unity antes de serem consideradas compatíveis na prática.

## Serviços e passos manuais

cloudProjectId,projectName eorganizationId estão vazios no projeto; UnityConnect está desativado. É necessário usar conta/organização Unity e vincular um projeto no painel, habilitar serviços/ambiente e conferir acesso/quotas. Não inventar IDs ou credenciais. Autenticação anônima deve ser inicializada pelo SDK; nenhuma chave de serviço/secreto será embutida no jogo.

Lobby organiza a sala e membros; Relay fornece conexão/atravessamento de rede; nenhum deles valida dano ou economia. Existem faixas gratuitas e cobrança por uso; consultar [preços oficiais](https://unity.com/products/gaming-services/pricing) e [regras de cobrança](https://docs.unity.com/en-us/services/pricing-and-billing) antes de ativar pagamento. Não contratar serviços pagos autonomamente.

Enquanto a configuração UGS depender do usuário, disponibilizar transporte direto privado para testar múltiplas instâncias no mesmo PC/LAN, rotulado claramente. Isso não equivale a convite pela internet nem crossplay real. iOS: código/plataforma devem ser preparados, mas exportação/assinatura final requer ambiente Apple e testes reais; nenhum binário iOS será alegado nesta máquina Windows.

## Implementação incremental

1. Resolver pacotes fixados e compilar, conservando testes locais.
2. Sala privada de até4, seleção de herói, início/carregamento/encerramento, primeiro em instâncias locais.
3. Autoridade do host sobre movimento, ataques, vida, IA e RNG; replicar apenas estados relevantes com IDs estáveis e pools visuais.
4. Masmorra/portas/objetivos compartilhados, impedir geração divergente e coordenar passagem de sala da equipe.
5. Recompensas únicas por partida/jogador, morte/reaparecimento, saída voluntária e reserva limitada para reconexão. Nenhuma migração de simulação do host será prometida: encerramento seguro quando ele sair.
6. Integrar MPS/Relay/Lobby com erro de configuração explícito; testar nuvem apenas com projeto/serviços reais disponíveis.
7. Testar2–4 clientes, rejeição do quinto, comandos repetidos/inválidos, latência, morte/loot, conclusão, desconexão e retorno. Teste PC–Android exige aparelho conectado; não substituir por duas instâncias Windows.

As etapas de interface completa/conteúdo/lançamento continuam depois deste recorte funcional e de suas limitações documentadas. Não usar matchmaking público ou sistemas sociais avançados.

## Bloco validado — base de autoridade
Pacotes resolveram/importaram e compilaram na Unity6000.3.26f1. Suite PlayMode executada:72/72 aprovados (68 anteriores +4 de comandos/party), XML em work/etapa7/foundation-tests.xml. CombatParty permite alvo vivo mais próximo e derrota somente com equipe sem sobreviventes. ValidatedHeroCommands rejeita NaN/infinito/eixos excessivos/bits inválidos, comandos repetidos/antigos e movimento sem atualização. PlayerMotor.Configure atualiza o adaptador em runtime; ResetCombat limpa somente efeitos do dono. Ainda não há sessão conectada validada neste checkpoint; implementação seguinte continua na etapa7.
