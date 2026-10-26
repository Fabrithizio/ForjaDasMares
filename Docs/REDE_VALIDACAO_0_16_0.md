# Validação de sessão — 0.16.0

## Ambiente
Duas instâncias reais do mesmo executável Windows, perfis/savepaths de QA isolados. Conexão direta loopback e depois Unity Relay real, na mesma conexão de internet. Não é teste entre redes diferentes, Android, quatro jogadores ou partida humana completa.

Na primeira tentativa Relay, a configuração de QA omitiu perfis de autenticação separados e o serviço recusou a entrada por identidade já pertencente à sala. O teste foi corrigido usando `-ugsProfile fdm0160h` e `-ugsProfile fdm0160c`; `-netIdentity` separa os perfis do jogo, mas não substitui o isolamento do SDK. Essa recusa não foi contabilizada como falha do jogo.

## Resultado observado
- Duas instâncias entraram emPlaying com a mesma assinatura lógica do mapa e os heróis Arcanista/Guerreiro.
- Ataque remoto eliminou um inimigo de teste na autoridade do host.2XP para os participantes; não houve cópia local independente do inimigo/recompensa.
- No teste direto, baú acionado via ferramenta de desenvolvimento:2→12XP para ambos. Segunda tentativa retornouFalse e XP/itens permaneceram iguais.
- Saída normal/retorno direto recuperou vaga, vida e2XP. Encerramento abrupto do processo cliente, reconhecido pelo host após cerca de30s, falhou na reconexão direta em duas rodadas. Causa não determinada; não declarar esse caminho resolvido.
- Pelo Relay, encerramento abrupto e nova instância com o mesmo perfil SDK/identidade/código retornaram aPlaying2/4, mesma vaga1,100HP e2XP. O host permaneceu ativo.
- Morte administrativa do host-jogador:HP0 sincronizado, aliado100HP e sessão continuouPlaying. Host restaurado administrativamente para prosseguir o QA.
- Resgate: aliado derrubado pela ferramenta; host aproximado e intenção de interação real enviada. Ambos viram aliado recuperado com35HP.
- Conclusão administrativa (sem batalha humana do chefe) levou ambos aVictory e106XP salvos. Repetir o comando não alterou XP nem inventário:6 itens no host/7 no cliente, com rolagens individuais.
- Retorno ao mapa e início da fase seguinte mantiveram o grupo e produziram a mesma nova assinatura nos dois clientes, com15 setores. Vida atualizada para74/103 pela progressão real.
- Derrota administrativa conjunta: ambos chegaram aDefeat; progresso salvo permaneceu106XP/36ouro, IDs dos itens únicos e resultado Derrota cooperativa nos dois arquivos de QA.
- Encerrar o host retornou ambos aClosed; limpeza das sessões Relay terminou sem operação pendente.

## Evidências locais
`Logs/Review0160` contém snapshotsplaying, baú/repetição, queda/retorno, morte/resgate, vitória/repetição, próximo mapa, derrota e encerramento. Logs das instâncias em `Logs/review0160-*.log`. Não publicar logs de autenticação, códigos ou saves privados no Git.

## Pendências prioritárias
- Investigar reconexão direta após fechamento abrupto. O retorno normal direto e o retorno abrupto Relay funcionaram; não inferir a causa da diferença sem diagnóstico.
- Testar PC–Android0.16.0 quando houver APK/aparelho disponíveis, redes distintas, quatro participantes e sessão completa jogada com seu irmão.
- A vitória administrativa testa transições/persistência; não comprova dificuldade, diversão, tempo de fase ou balanceamento do chefe.
- Host dedicado/migração de host continuam não implementados. A saída do host encerra a sala atual.
