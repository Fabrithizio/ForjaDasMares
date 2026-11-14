# Verificação complementar de latência — 14/10/2026

## Execução real

Dois executáveis Windows cooperativos0.6.1, sem alterar gameplay ou recompilar, conectados por um proxy UDP exclusivamente em loopback. Host na porta7780, proxy7779; saves e logs isolados do perfil normal. O host/celular normais permaneceram separados na porta7777.

`Tools/QA/udp_latency.py` aplica75ms em cada direção, jitter uniforme±20ms e perda aleatória2%, seed12345. Isso adiciona aproximadamente150ms de ida e volta ao transporte local; não é medição de ping de internet nem simulação de congestionamento/banda limitada. A API antiga SetDebugSimulatorParameters da versão instalada é obsoleta e sem efeito; por isso não foi usada para alegar um teste artificial inexistente.

## Resultados observados

- Dois processos entraram em Playing e confirmaram a mesma assinatura do mapa. Movimento remoto registrado:1,716m pelo probe.
- Cliente atacou e matou o goblin real; log do host confirmou COOP_PROBE_REMOTE_KILL. XP12 para ambos.
- Comando administrativo de QA acionou o baú real: XP22 para ambos. Repetição retornouFalse, manteve22XP. Esse teleporte/uso controlado não comprova navegação humana.
- Derrota controlada finalizou o resultado:11XP em ambos,13 itens no host e14 no cliente. Contagens distintas são permitidas pelo loot individual; o cliente só recebeu seu próprio perfil.
- Encerramento do host levou o cliente aClosed, zero conectados. Nenhuma Exception ou Error nos dois logs examinados.
- Amostra de105s do proxy:6765 datagramas recebidos,6614 encaminhados,148 descartados,3 ainda em fila;1625860 bytes recebidos somando os dois sentidos, incluindo cabeçalhos/retransmissões. Não é estimativa de tráfego de quatro jogadores ou combate intenso.

Provas locais: `work/etapa7/latency/proof` no workspace Codex, com snapshots de morte do alvo, baú, repetição, derrota e encerramento; logs em `work/etapa7/latency`. Atraso perceptível e qualidade visual da interpolação ainda exigem avaliação humana. Não houve teste físico Android com esse proxy, quatro clientes sob perda, reconexão sob perda ou vitória humana completa.

## Reproduzir

Usar Python3 com biblioteca padrão. Iniciar o proxy:

```text
python Tools/QA/udp_latency.py --report CAMINHO_ABSOLUTO/proxy.json --seconds 180
```

Iniciar o executável do host com `-netMode host -netPort 7780 -netPin TEST42 -netIdentity 11111111111111111111111111111181 -netProbe CAMINHO_ABSOLUTO/host -netExpected 2 -netSavePath CAMINHO_ABSOLUTO/profiles -logFile CAMINHO_ABSOLUTO/host.log`. Iniciar cliente com `-netMode client -netAddress 127.0.0.1 -netPort 7779 -netPin TEST42 -netIdentity 11111111111111111111111111111182 -netProbe CAMINHO_ABSOLUTO/client -netExpected 2 -logFile CAMINHO_ABSOLUTO/client.log`.

Os caminhos devem existir e ser devidamente citados pelo shell quando contiverem espaços. Flags de probe só funcionam em Development Build/Editor. O proxy aceita somente um endpoint de cliente, encerra após o tempo configurado e não altera configurações de rede/firewall. Não faz parte do jogo distribuído.

## Estado da etapa

**Aprovado com limitações para testes privados na LAN.** Internet/Relay continua pendente de conta/projeto Unity Services real e teste entre redes distintas. A etapa7 inteira não foi declarada concluída. Não avançamos a interface completa nem à rodada geral de bugs e melhorias nesta verificação. PC e Android normais foram novamente conectados em Lobby2/4 para teste do usuário; o host deve pressionar INICIAR PARTIDA.

Arquivos desta rodada: este relatório, `PROGRESSO.md`, `REDE_PROGRESSAO.md` e `Tools/QA/udp_latency.py`. Nenhum script C#, cena, pacote ou build mudou; não foi necessário recompilar a Unity. Testado o script executando tráfego real, não apenas por inspeção.
