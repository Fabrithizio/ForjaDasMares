# Rede — bloco de sessão e simulação privada (etapa 7 em andamento)

## Evidências em 14/10/2026

- Dependências reais importadas: NGO2.13.3, Transport2.7.4 e Multiplayer Services2.4.0; compatíveis com a compilação executada na Unity6000.3.26f1.
- Base de comandos/party:72/72 PlayMode (68 anteriores+4 novos).
- Cena/protocolo cooperativo:6/6 PlayMode, incluindo adaptadores únicos, serialização, tamanho de payload após cabeçalho NGO, masmorra real/derrota e destruição da raiz de rede ao voltar para cena offline.
- Build de teste Windows: Builds/CoopWindows/ForjaDasMaresCoop.exe, COOP_BUILD Succeeded errors=0. Executado em processos separados com transporte UDP local, não apenas em mocks.
- Host+3 clientes carregaram o mesmo grafo/seed; confirmação de layout antes de habilitar combate. Quinta conexão recusada.
- Movimentos enviados pelo cliente alteraram as posições no host; um cliente derrotou um goblin real com ataques básicos. Health/damage/AI simulados somente no host; replicas não têm controlador de colisão nem ataque ativo.
- Morte do herói do host não terminou a sessão enquanto havia aliados vivos. Saída de cliente reduziu grupo a3; reconexão recuperou a mesma vaga (slot3) e o estado de vida. Saída do processo host encerrou os3 clientes; nenhum deles assumiu autoridade.
- Nenhuma exceção nos logs da terceira execução de4 clientes. Primeiras execuções encontraram duplicação de adaptador, ordem de criação da UI e leitura incorreta do tamanho por incluir o hash NGO; corrigidos e cobertos por testes.

Logs/probes em work/etapa7/third-*/ e proof-third/ no workspace Codex. Os testes são automáticos em Windows headless; ainda não constituem QA visual desta cena nem teste de Android/iOS, latência de internet ou quatro pessoas jogando.

## Como executar este recorte

Abra Assets/ForjaDasMares/Scenes/CoopDungeon.unity e Play. Escolha herói antes da partida. Para teste no mesmo PC, abra várias instâncias do executável; host cria sala local, clientes usam127.0.0.1, porta7777 e o mesmo código. O limite inclui o host:4 pessoas. Em LAN, use o IP local real do host e a mesma porta/código. Não altere firewall ou exponha portas automaticamente. WASD/setas movem, mouse aponta/ataca, tecla de habilidade/esquiva existentes e E interage. UI de toque envia intenções pelo mesmo adaptador; não validada em aparelho nesta versão.

Ao entrar numa sala, o grupo é reunido em posições internas para permitir reutilizar o controlador de encontro único sem trancar aliados no corredor. Jogadores mortos aguardam o fim; ainda falta o fluxo cooperativo completo de reaparecimento e recompensa. O host pode encerrar; não há migração de host. Reconexão reserva a vaga por90s enquanto o mesmo host continua ativo.

## Serviços online e limites

CoopCloud usa Authentication anônimo e Sessions privados via SDK MPS/Relay, sem chaves secretas. Botões de criar/entrar por código executam o fluxo real e retornam falha de configuração se o projeto não estiver vinculado. Não foi feita chamada online bem-sucedida: cloudProjectId continua vazio. Precisa vincular conta/organização/projeto em Unity Services, habilitar ambiente/Authentication/Lobby/Relay e conferir quotas/condições/cobrança nos links do plano de migração. Não foi contratado serviço pago.

O código de sala/IP atende sessões privadas. A identidade aleatória de reconexão e o PIN de LAN não equivalem a identidade comercial validada por backend; LAN direto não oferece a proteção de Relay. Host é confiado na simulação, mas pode adulterar o próprio executável. Economia comercial exige servidor/backend de confiança. Os arquivos locais não são autoridade online.

## Ainda falta nesta etapa

Integração autoritativa de progressão/saques/equipagem, resultado e reaparecimento cooperativos; revisão das interações/recompensas simultâneas; sessão completa contra ondas/chefe; medição de tráfego e teste de latência; QA visual e teste físico PC–Android. Os quatro heróis usam dados-base no recorte atual. Inventário local0.5.0 continua funcional nas cenas ProgressionDungeon/ProgressionArena e não é convertido implicitamente em autoridade de rede.

Snapshots limitados a48KiB, envio10Hz ajustável, entradas30Hz/rate limit45/s; interpolação das replicas. Há alocação limitada pela serialização JSON; otimização e medição fazem parte da continuação. Não alegar predição/reconciliação avançada, economia segura, convite real pela internet ou crossplay já testados. Interface completa, expansão e lançamento aguardam a etapa7, conforme ordem autorizada.
