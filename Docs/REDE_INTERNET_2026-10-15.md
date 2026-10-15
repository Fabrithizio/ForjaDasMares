# Integração de internet — 15/10/2026

## Configuração real

O titular concluiu o login no Unity Cloud. Foi criado um projeto separado **ForjaDasMares**, Project ID público `d8fae52c-f0ba-4d51-8e2b-faf465d77c7a`, organização4673123702332. Identificadores gravados em ProjectSettings; nenhum token/senha foi colocado no fonte. A configuração inicial do painel de projeto principalmente infantil permaneceu no padrão No; público/classificação e Developer Data Settings precisam de revisão antes da publicação. Nenhuma assinatura paga foi contratada e nenhum projeto antigo foi alterado.

CoopInstaller.InternetWindows/InternetAndroid validam o vínculo/configuração antes da build e usam pastas separadas, versão0.6.2/code8. QA continua Development Build; Android target35, sem AAB comercial. O plano de servidor dedicado/Play Store está em INTERNET_E_PUBLICACAO.md.

## Evidência inicial no serviço real

- Windows0.6.2 compilou Succeeded/errors0 com destino ativo Win64.
- Host autenticou anonimamente, criou uma sessão privada MPS/Lobby/Relay e recebeu código real. Outro processo Windows, com perfil SDK separado, entrou pelo código.
- Ambos chegaram aPlaying2/4, mesmo layout confirmado. Probe registrou movimento de1,70m no host e1,83m no cliente.
- Ataques reais do cliente derrotaram goblin da simulação; host confirmou COOP_PROBE_REMOTE_KILL. Ainda é combate automatizado de QA, não sessão humana completa.
- Nenhuma Exception/Error nos dois logs examinados dessa primeira execução. Artefatos locais em work/etapa7/relay-real; códigos de sala e dados de perfis não são publicados no Git.

## Falha encontrada e correção em validação

A primeira volta após encerrar o processo cliente foi recusada por `player is already a member of the session`. A entrada simples por código não resolve uma associação ainda mantida pelo Lobby. O processo pode terminar antes de LeaveAsync completar.

CoopCloud agora guarda somente sessão/código por projeto/ambiente/perfil SDK em PlayerPrefs, chama ReconnectToSessionAsync quando o código coincide e recorre à entrada normal somente quando a associação/sala já não existe ou não permite reconexão. A autoridade continua validando a reserva90s e o estado do jogador. O cache não concede identidade, itens ou moeda; autenticação continua pelo SDK. Ao confirmar Leave, remove o cache. Novas operações aguardam a limpeza anterior.

Não há migração de host nem retomada de simulação do host encerrado. Instâncias antigas sem esse cache não recebem reconexão retroativamente. A primeira build Android foi cancelada durante preparação/importação para incluir a correção; nenhum APK novo foi entregue como se contivesse código ainda não compilado.

Dois Windows na mesma conexão acessando Relay real não comprovam PC–Android ou redes/operadoras distintas. Só declarar esses testes após executá-los.

## Regressão anterior à correção

Com o projeto vinculado,20 testes de rede executados:19 aprovados,0 falhas,1 ignorado porque exige projeto sem vínculo para verificar bloqueio offline. XML internet-linked-tests.xml. O teste não fez chamadas de UGS. A execução anterior sem vínculo foi20/20 aprovada; os resultados são checkpoints diferentes. Novo teste de correspondência do cache e nova regressão serão registrados após a compilação.

## Correção validada no Relay — Windows

- Nova build Windows0.6.2 com cache/reconexão: Succeeded/errors0, log internet-windows-resume.log.
- Regressão21 testes:20 aprovados,0 falhas,1 ignorado (caso de projeto sem vínculo), XML internet-resume-tests.xml. O caso de cache verifica que outro código, ID vazio ou excessivo não permitem retomar a referência.
- Sessão real nova em Relay: dois clientes chegaram aPlaying e o cliente matou goblin,12XP em ambos.
- Processo cliente encerrado abruptamente por QA, sem executar Leave. Host reconheceu a queda. Nova instância com a mesma identidade/perfil SDK voltou à vaga1, Playing2/4,140HP e12XP preservados. Entre captura anterior à queda e posterior ao retorno decorreram59s; reserva é contada após a desconexão reconhecida pelo host, não desde a captura. Provas em work/etapa7/relay-resume/proof.
- Baú real acionado administrativamente:22XP para ambos; segunda utilizaçãoFalse e22XP preservados. Derrota controlada:11XP e13 itens em cada perfil, resultado sincronizado. Não representa vitória humana de masmorra.
- Sem Exception/Error nos logs examinados de host e cliente retornado. Testes reais não substituem os riscos comerciais de autenticação/host descritos no plano.

Android com essa correção está em compilação; não foi declarado testado nesta revisão ainda.
