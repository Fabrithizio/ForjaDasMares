# Internet, servidor e futura publicação

Atualizado em15/10/2026. Este é um protótipo privado; não representa aprovação de publicação nem validação de Relay.

## Conectar pela internet

1. Entrar pessoalmente em https://cloud.unity.com/ com a conta do Unity Hub. Criar/selecionar organização e projeto ForjaDasMares. Não contratar hospedagem nem habilitar cobrança sem revisar os custos.
2. No Editor, Project Settings > Services, vincular ao mesmo projeto/organização. O resultado deve preencher Application.cloudProjectId; não usar ID inventado. Project ID é identificador público, não senha. O login é feito pelo titular.
3. No painel, conferir acesso a Multiplayer Services/Sessions, Authentication, Lobby e Relay; selecionar o ambiente. `production` é o valor padrão do cliente e deve existir no painel. Verificar quotas e eventuais termos/avisos exibidos. Nada disso foi ativado automaticamente nesta entrega.
4. Menu **Forja das Mares > Internet > Configuração** seleciona `Assets/ForjaDasMares/Resources/CoopInternet.asset`: ambiente, região opcional e timeout. Região vazia usa QoS/seleção automática do SDK. PC e Android precisam usar o mesmo projeto, ambiente, protocolo e dados compatíveis.
5. Gerar novos clientes depois do vínculo. Os binários0.6.1 anteriores não adquirem esse vínculo automaticamente. Manter os APKs de teste distintos de uma futura versão de publicação.
6. Host: CRIAR SALA INTERNET. Cliente: copiar o código retornado e ENTRAR POR CÓDIGO. A sala é privada e comporta quatro pessoas contando o host. Relay evita exigir IP público ou abertura manual de portas no roteador.
7. Validar em redes realmente distintas: PC na conexão fixa e Android em dados móveis, com consentimento do usuário para usar sua franquia. Não desativar Wi-Fi por automação. Registrar criar/entrar/combate/loot/resultado/saída/reconexão. Só depois classificar internet como testada.

**Estado real:** conta/projeto ainda não vinculados; tela Unity Cloud abriu em login. Nenhuma chamada ao serviço real foi validada. O código não contém senhas, service-account keys ou tokens de servidor. A autenticação anônima cria identidade de teste; contas recuperáveis e validação de identidade na autoridade ainda precisam ser implementadas.

## Como a configuração funciona

CoopInternetSettings concentra apenas parâmetros públicos. CoopCloud lê o asset uma vez na criação, verifica GUID real do projeto e configuração válida, inicializa UGS com SetEnvironmentName, autentica e chama MPS com WithRelayNetwork. MaxPlayers4 permanece fixo pelo protocolo. Não modificar o ambiente durante uma sessão inicializada; reiniciar o processo após mudanças.

O menu de configuração cria o asset se necessário, sem regravar a masmorra. Falta de vínculo produz mensagem e preserva o modo offline/LAN. Timeout/cancelamento fazem a limpeza existente. O SDK pode demorar a retornar de uma operação interna antes de observar cancelamento; não prometemos interrupção imediata. Não há migração do host. Sair do host encerra a partida com segurança conforme testes LAN; reconexão do SDK/Relay ainda exige teste real, não é comprovada pelo retorno direto em LAN.

## Caminho para servidor dedicado

**Hoje:** o jogador host também executa simulação e grava perfis cooperativos. Relay transporta mensagens, não executa IA, dano ou economia. Um host modificado pode adulterar seus resultados; GUID/PIN de LAN não prova identidade. Publicar na loja não elimina esses limites.

**Pontos de extensão já existentes:**

| Responsabilidade | Código reutilizável | Migração ainda necessária |
|---|---|---|
| Transporte/sala | CoopCloud + UnityTransport + mensagens NGO | Provedor de alocação deve entregar endereço/porta e credencial curta; cliente não escolhe autoridade arbitrária para economia comercial. |
| Simulação | PlayerMotor, HeroCombat, EnemyBrain, DungeonSession e ValidatedHeroCommands | Extrair a inicialização de host em CoopDirector para papel server-only. Atualmente IsHost e jogador local0 estão acoplados; StartServer sozinho não implementa servidor dedicado. |
| Participantes | Approval/Seat/limites de comando | Vincular cada conexão a identidade autenticada, validar ticket no servidor e impedir apropriação de vaga por GUID. Não embutir segredo de assinatura no app. |
| Recompensas | CoopRewardLedger e ProfileService | Repositório de perfis autenticado e transacional no backend, idempotência durável por partida/evento/jogador; não importar inventário do cliente. Arquivos atuais continuam apenas protótipo privado. |
| Persistência/versões | Envelope de save e protocolo2 | Definir migração auditada de contas/catálogo e compatibilidade entre versões. Hash atual do mapa não valida todo o catálogo de combate. |
| Operação | Tests de regras/sessão e logs de desenvolvimento | Build headless, hospedagem, monitoramento, limites de custo, atualizações e testes de queda/reinício/escala. Nenhum servidor foi contratado ou produzido. |

Essa separação permite conservar as regras de jogo na migração, mas não significa troca instantânea de uma URL. Não foram adicionadas interfaces vazias ou implementações falsas de backend. Antes de economia pública, completar autenticação/tickets/persistência confiável e testes de múltiplos clientes. Para testes privados iniciais, Relay com host é o caminho de menor infraestrutura.

## Play Store: requisitos a validar na etapa de lançamento

Consulta oficial em15/10/2026:

- [Target API](https://developer.android.com/google/play/requirements/target-sdk): a documentação atual exige API36 para novos apps/updates de telefone. O APK atual usa target35 e não está pronto para nova submissão. Atualizar o destino em build de publicação e testar mudanças de comportamento; o builder de QA não foi alterado silenciosamente.
- [Android App Bundle](https://developer.android.com/guide/app-bundle): novos apps usam AAB. Hoje só produzimos APK de desenvolvimento, não AAB de produção.
- [Páginas de16KB](https://developer.android.com/guide/practices/page-sizes): validar todas as bibliotecas nativas e alinhamento do pacote no artefato final; ARM64 sozinho não comprova essa compatibilidade.
- Conta Play Console, identificador definitivo, assinatura/upload key e recuperação protegida, Play App Signing, versionamento crescente e trilha de teste. Não salvar keystore/senhas no Git público; não publicar antes de revisão.
- Revisar Data safety/privacidade com base nos SDKs efetivamente usados, autenticação, retenção/exclusão de dados e públicos/regiões de distribuição. Unity Authentication também exige revisar [notificações DSA](https://docs.unity.com/en-us/mps-sdk/get-started) conforme orientação atual da Unity.
- Fechar os testes funcionais, ergonomia móvel, desempenho em aparelho modesto, reconexão/background, limites de tráfego e sessão completa. Nenhuma aprovação universal ou comercial foi declarada.

Referências técnicas: [MPS](https://docs.unity.com/en-us/mps-sdk/get-started), [Relay/Lobby](https://docs.unity.com/en-us/mps-sdk/networking/relay-integration), [preços UGS](https://unity.com/products/gaming-services/pricing). Rever requisitos na data de publicação.
