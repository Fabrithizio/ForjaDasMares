# Atualização 0.14.1
Saídas fixas Builds/Atual/Windows/ForjaDasMaresCampanha.exe e Builds/Atual/Android/ForjaDasMaresCampanha.apk. Protocolo 10/save v2; Android código 29. Relatório atual: REVISAO_0_14_1_2026-10-21.md. Teste físico PC–moto g15 executado no mesmo Wi-Fi; não desinstalar a campanha atual. Instruções anteriores abaixo continuam válidas, exceto números de versão históricos.

# Versão 0.14 — instruções atuais

Abra o atalho único **Jogos/Jogar Campanha Forja das Mares** depois da atualização. O executável atual fica em Builds/Atual/Windows/ForjaDasMaresCampanha.exe; mantenha a pasta inteira com seus arquivos de dados. APK atual: Builds/Atual/Android/ForjaDasMaresCampanha.apk. Atualize a instalação com.forjadasmares.campaign; não limpe os dados. A pasta antiga de envio ao irmão não foi atualizada nesta rodada.

Todos precisam da mesma versão/protocolo10 para jogar. No início, PERSONAGEM abre mochila, equipamentos e talentos sem precisar criar sala. Itens equipados aparecem como imagem nos slots e saem da grade da mochila. Clique no slot para detalhes; comparação e reciclagem confirmada continuam disponíveis.

Segure o ataque para repetir o combo; soltar a carga avançada prepara o golpe pesado. A esquiva perfeita concede um próximo ataque mais forte por uma janela curta. Machados/adagas aplicam sangramento; martelos fraturam defesa; cajado/arma mágica queimam; a arma especial de puxão reúne alvos. Esses efeitos respeitam as restrições de classe e os limites descritos na revisão.

Explore os altares em segurança e escolha Poder, Ritmo ou Abrigo (uma escolha por altar/jogador, só nesta expedição). Esconderijos dão prêmio único ao grupo. Barris avisam antes de explodir e ferem ambos os lados. Paredes rachadas podem ser quebradas. Pontes frágeis expõem perigo sinalizado; a alavanca do setor reforça/desarma a passagem.

O desafio opcional pede confirmação quando estiver perto e sem ameaças. Catacumbas: ondas; floresta: proteger selo; fortaleza: escoltar guia. Acompanhe vida, tempo e progresso. Sair da área, perder objetivo ou esgotar tempo falha o evento; a campanha continua. Não há prêmio repetido por reabrir o mesmo evento.

G/direcional↑ marca AQUI. Os botões AQUI/ALVO/PERIGO/SAQUE funcionam no PC e toque; marcações também aparecem no minimapa. Nas configurações há volume de efeitos, vibração e impacto de câmera opcionais. Para rede, use criar/entrar por código; configurações Unity Services anteriores são preservadas. Sem migração de host: se ele sair, a sala termina com segurança.

Testes e limitações efetivamente verificados nesta revisão: REVISAO_0_14_2026-10-20.md. Não usar as instruções de envio/versionamento dos registros históricos abaixo.

---

# Jogar a campanha 0.12.0 (PC)

## PC e Android juntos

1. PC atualizado: **Jogos/Jogar Campanha Forja das Mares**. Esta revisão usa protocolo 8. O APK 0.11 anterior usa protocolo 7 e não pode entrar numa sala 0.12; nova geração/distribuição de APK foi adiada a pedido do usuário. A pasta de envio ao irmão não foi atualizada.
2. Ambos precisam de internet e da mesma versão. Host: CAMPANHA → fase → CRIAR SALA PRIVADA. Compartilhe o código exibido.
3. Convidado: CAMPANHA → informe o código → ENTRAR PELO CÓDIGO. Host: INICIAR COM O GRUPO. Prefira aguardar todos no lobby.
4. Pode iniciar sozinho, sem bônus de vida ou dano. Uma sala publicada recebe jogadores até 20% do percurso e no máximo quatro; uma sala privada depende de código.
5. MENU/Esc abre sala, convites, mapa e opções durante a fase. Não pausa o online. Inventário possui equipamento, comparação e reciclagem confirmada por ouro; o resultado mostra os itens obtidos.
6. Mantenha o host aberto; sair encerra a sessão. O host guarda a progressão cooperativa. Não há migração de host nem transferência automática de progresso entre hosts. Amigos são agenda local de convites, não uma rede social com presença.

## Perfil e equipamento antes de entrar

- No início, abra **PERSONAGEM · EQUIPAMENTOS E MOCHILA**. Escolha herói e use TODOS/ARMAS/SECUND./ARMADURA/ACESSÓRIO/ARTEFATO; o filtro de classe começa ativo.
- Os cinco slots aparecem ao redor do avatar. Equipados saem da grade da mochila; desequipar os devolve. Mochila cheia impede desequipar sem perder a peça. Selecione um item para comparar atributos, classe/nível e habilidade. Adagas compatíveis oferecem USAR NA SECUNDÁRIA quando permitido; uma arma de duas mãos retira a secundária para a mochila.
- Cada classe recebe uma arma inicial comum, fraca e focada em um alvo. Ouro e XP são concedidos imediatamente por morte; equipamento é aleatório, com melhores chances nas fases mais difíceis. Não há mais arma gratuita por concluir a primeira sala. Confira **SAQUE ATUAL**, compare e equipe fora de perigo; nada é equipado automaticamente.
- O contexto **ESTE APARELHO** é usado quando você hospeda. **ÚLTIMA SALA** permite organizar o último perfil recebido de outro host: alterações de equipamento são revalidadas ao voltar ao mesmo host, no lobby. Aguarde a sincronização antes de iniciar. Não transfere itens entre hosts nem representa uma conta global.
- Saves antigos são mantidos; itens que ficaram incompatíveis com a classe voltam para a mochila. Atualize o APK por cima, sem limpar os dados do aplicativo.

## Níveis, itens e ouro

- Cada nível após o primeiro libera **1 ponto por herói**. No perfil, abra **ATRIBUTOS E TALENTOS**. Escolha Vigor, Força, Tenacidade, Agilidade, Precisão ou Disciplina. Cada melhoria tem limite de cinco pontos. As regras ficam no catálogo de dados.
- Para redistribuir, confirme o custo de **25 ouro por ponto já investido**. Investir e reciclar funciona no seu perfil deste aparelho ou conectado ao host do perfil da sala, fora de perigo. O cache ÚLTIMA SALA sem o host não pode conceder ouro/talentos.
- Clique/toque na peça da mochila ou num slot para ver atributos próprios, alcance, sequência, habilidade, requisitos e ouro de reciclagem. A comparação começa pelo item equipado no mesmo slot; **COMPARAR COM OUTRO ITEM** percorre outras peças possuídas desse slot.
- **RECICLAR** exige confirmação e devolve ouro apenas uma vez. Desequipe antes de reciclar. O ouro fica salvo e é a moeda base; não há loja de dinheiro real nesta fase.
- Todos os inimigos mortos concedem ouro e XP pelo host, inclusive invocações finitas com prêmio menor. Tokens de drop são recolhidos automaticamente e não disputados entre aliados. Equipamentos aleatórios aparecem em SAQUE ATUAL para equipar fora de combate; são guardados ao terminar a expedição. Créditos imediatos não se repetem no resultado e são mantidos na derrota.
- Fases inicial/veterana/desafiadora: inimigo comum concede 2/3/4 ouro e XP, com chance de equipamento de 10/14/19%. Desafiadora usa raridade mínima incomum. Chefes, elites e baús usam suas tabelas próprias. Capacidade de saque pode limitar equipamento adicional; ouro/XP de mortes continuam.

## HUD e orientação

- PC: I abre/fecha personagem; M amplia o minimapa; Esc/MENU abre opções e código da sala. Barra com ícones: ataque, E habilidade da arma/classe, Espaço esquiva, F interação, Q super/Fúria, I personagem.
- Celular: toque no minimapa para ampliar; os controles de combate continuam nas laterais. FÚRIA mostra a duração restante quando ativa.
- O minimapa compacto fica no topo esquerdo; vida e retrato ao lado. O menu da sala mantém código e opções.
- Vermelho marca inimigos e setor pendente; azul/ciano, grupo/jogador; dourado, objetivo. A distância ao inimigo mais próximo ajuda a encontrar remanescentes.
- Flechas têm emissores visíveis com preparação; inimigos mostram um anel de chegada antes de agir. São recursos provisórios, não arte final.

## Novidades para experimentar

- Quebre suprimentos para obter recompensas e curar um aliado ferido próximo. A cura acontece uma única vez; procure a marca verde.
- Encontre e destrua os três selos: equipamento raro ou melhor, XP e moedas, uma vez por expedição.
- INTERAGIR perto de uma fonte recupera 35% da vida máxima dos aliados vivos no mesmo setor, fora de combate; cada fonte é de uso único.
- Guerreiro: segure ataque para carregar o golpe pesado (1,2s; intervalo 4s). FÚRIA enche após 24 acertos válidos; Q no PC/botão FÚRIA no toque ativa 6 segundos de cadência ×1,55, sem explosão em área. Durante a Fúria, ataques não recarregam a barra.
- Corta-Maré, Salva de Junco e Foco de Brasa mudam arco/alcance, disparos em leque e explosão, respectivamente. Os visuais ainda são provisórios. As demais armas também têm sequências configuradas de três ataques.
- Patrulheiro, Arcanista e Cavaleiro Rúnico também têm ataque carregado (1,15s) e SUPER após24acertos. Guerreiro também exige24acertos.
- Na mochila, SAQUE ATUAL mostra itens encontrados nesta tentativa. Se estiver fora de perigo, selecione e equipe para alterar o combate durante a fase. Não é preciso esperar o resultado.
- As fases têm setores28/30/32m e passagens abertas, sem portões fechando. Encontros difíceis acumulam109/136/164inimigos em ondas; o limite simultâneo segue o host:20noAndroid/24noPC.
- Três elites possuem ataques de preparação visível. Derrote os combates e conclua os objetivos obrigatórios antes de usar a saída final. Só há um encontro ativo por vez; o minimapa destaca o setor pendente e os inimigos restantes. Reforços intermediários podem chegar com até dois remanescentes; a última onda exige eliminar todos.
- MAPA E OBJETIVOS mostra os setores reais, sua posição e progresso; o HUD informa cooldowns e personagem caído.

Cena do Editor: `Assets/ForjaDasMares/Scenes/CampaignCoop.unity`. Pressione Play após terminar a importação. As cenas de arena e masmorras anteriores foram preservadas.

Builds: `Builds/CampaignWindows-0.11.0/ForjaDasMaresCampanha.exe` e `Builds/CampaignAndroid-0.11.0/ForjaDasMaresCampanha.apk`. APK ARM64, Android mínimo API25, assinado para desenvolvimento; não é release da Play Store. Atualize por cima para preservar os dados. O Android pode pedir autorização pessoal para instalar o arquivo recebido.

Ainda exige sessões humanas completas para balancear dificuldade/duração, outros modelos de celular e testes entre redes diferentes. Esta é uma versão privada de testes, com arte provisória em parte do conteúdo. Evidências e dependências: `PAISAGENS_E_HORDAS_0_10_2026-10-17.md`.

## Histórico de instruções anteriores

# Atualização 0.8.2

PC: atalho Jogar Campanha Forja das Mares. Android: enviar como Documento o APK em `Área de Trabalho/Jogos/Enviar para meu irmao/ForjaDasMares-0.8.2-Android.apk`. Crie a sala no PC e compartilhe o código; convidado usa Campanha → Entrar pelo código. Prefira entrar antes de iniciar. Na fase: código no alto e botão MENU para grupo, convite, opções e saída. Menu não pausa o online. Amigos são agenda local de convites.

# Campanha 0.8.1 — solo e cooperativo

## Abrir

PC: abra o atalho **Jogar Campanha Forja das Mares** na pasta Jogos da Área de Trabalho, ou execute `Builds/CampaignWindows-0.8.1/ForjaDasMaresCampanha.exe`. Cena do Editor: `Assets/ForjaDasMares/Scenes/CampaignCoop.unity`. Todos precisam usar a versão0.8.1 da campanha; versões antigas não entram nesta sessão. A tela inicial tem CAMPANHA, AMIGOS E CONVITES, CONFIGURAÇÕES e saída confirmada.

Android: APK da revisão em `Builds/CampaignAndroid-0.8.1/ForjaDasMaresCampanha.apk`, aplicativo **Forja das Mares - Campanha**, separado das Galerias. Android ARM64, mínimo API25. Consulte o relatório da revisão para o estado real de compilação/instalação/testes. Não é um pacote aprovado para a Play Store. Para compartilhar o Windows, envie a pasta de build inteira.

## Você e seu irmão

1. Ambos abrem a campanha com internet.
2. Quem vai hospedar escolhe a fase no mapa e usa CRIAR SALA PRIVADA.
3. Envie ao parceiro o código mostrado (COPIAR CÓDIGO copia para a área de transferência).
4. O parceiro informa esse código e usa ENTRAR POR CÓDIGO.
5. Cada um escolhe seu herói. O host pode iniciar com1–4 pessoas. Sozinho recebe +10% de vida máxima e dano; a entrada de outro jogador remove esse bônus sem conceder cura.
6. Após o resultado, o host usa CONTINUAR CAMPANHA, escolhe outra fase e inicia novamente. O grupo e o código são preservados.

O Relay dispensa informar IP local ou abrir portas no roteador. Isso não garante funcionamento em toda rede: falhas de conexão precisam ser investigadas com os logs. Se o Windows apresentar um aviso de Firewall, responda pessoalmente conforme a rede em que confia.

## Encontrar parceiros

Escolha a fase e use PROCURAR GRUPO DESTA FASE. O jogo procura uma sala pública nessa fase, aguardando ou já jogando com menos de20% concluídos; se não houver vaga, cria uma sala pública que também pode iniciar solo. O host pode usar ABRIR SALA PARA GRUPOS ou TORNAR SALA PRIVADA no mapa e durante a partida. O limite continua4. Não existem jogadores automáticos.

Progresso significa setores concluídos, excluindo a entrada. Aos20% ou mais, nenhuma nova pessoa entra, mesmo com código; reconexão de participante já reservado é diferente e continua disponível. Quem chega recebe o mapa/estado atual e recompensas futuras, sem receber retroativamente os baús já abertos.

Cada participante mantém sua vaga durante a tentativa, inclusive ao desconectar por mais de90s, para voltar ao mesmo perfil sem duplicar o save. O limite4 inclui essas reservas: não é possível substituir indefinidamente jogadores na mesma tentativa. O retorno depende de a sessão do serviço ainda existir; saída do host encerra tudo. Nas variantes cooperativas antigas, a reserva continua seguindo a janela antiga.

AMIGOS E CONVITES guarda neste aparelho até6 nomes e códigos compartilhados. Toque num convite salvo para conectar. Não existe ainda conta de amigo, presença online ou convite remoto automático; o código expira quando o host fecha a sala.

## Controles e desafios

- PC: WASD/setas para mover; mouse para mirar; segure ataque para repetir golpes; E para habilidade; Espaço para esquiva; F para interagir/resgatar. Use MOCHILA para equipamentos.
- Android: joystick esquerdo e botões de ataque, habilidade, esquiva e interação à direita. Mantenha ataque pressionado para repetir.
- Entrem juntos nos setores: o encontro começa quando os heróis vivos estão dentro. Derrotem as ondas para liberar as portas.
- Em grupo, cada placa requer uma pessoa viva diferente: mantenham as duas pressionadas. Solo: visite as duas em sequência para ativar o mecanismo. Entrar/sair alguém zera a ativação parcial, preservando mecanismos já concluídos.
- Linhas de flechas avisam antes de disparar. Baús e saída são usados com interação próxima.
- Fora de combate, aproxime-se de aliado caído e interaja para resgatá-lo. Uma parede impede o resgate.
- A mochila da campanha começa vazia; o ataque base do herói funciona sem uma arma equipada. Baús/chefes são fontes mais confiáveis de equipamento que inimigos comuns.
- Se a mochila lotar, itens excedentes ficam em ENTREGAS. Libere espaço e receba-os. Equipar respeita as restrições da classe; descarte exige confirmação.

## Salvamento e limites importantes

O progresso é guardado no PC/aparelho que hospeda, em perfis separados dos saves antigos. Não é uma conta global sincronizada entre hosts. Joguem com o mesmo host para manter essa continuidade. Fechar o host encerra a partida de todos; não existe migração de host.

As três fases estão liberadas para teste, com12,15 e18 setores. A duração desejada da primeira é cerca de5–7 minutos em dupla, ainda sem medição de uma vitória humana completa. O chefe existente e os cenários são provisórios. Brasa possui o modelo/animações autorais; as demais classes continuam com placeholders. O pesado e SUPER da variante local das Galerias ainda não são recursos desta campanha online.

Na revisão0.8.1, dois clientes Windows pelo Relay testaram início solo, abertura da sala e entrada com a partida em andamento, remoção do bônus e bloqueio após20%. Consulte CAMPANHA_SOLO_2026-10-17.md para resultados finais do Android. Quatro dispositivos, redes distintas e vitória humana cronometrada continuam pendentes.

Detalhes técnicos, testes e arquivos: [relatório da campanha](CAMPANHA_2026-10-17.md).

## Gerar novamente a versão atual
Com a Unity fechada, abra PowerShell na pasta do projeto e execute (ajuste somente o caminho do Editor se mudar a instalação):

```powershell
$project = (Resolve-Path '.').Path
& 'C:\Program Files\Unity\Hub\Editor\6000.3.26f1\Editor\Unity.exe' -batchmode -nographics -buildTarget Android --burst-force-sync-compilation -projectPath $project -executeMethod ForjaDasMares.Networking.Editor.CampaignInstaller.Android -quit -logFile "$project\Builds\Atual\android-build.log"
```

Para Windows, troque Android por Win64 em buildTarget e use executeMethod ForjaDasMares.Networking.Editor.CampaignInstaller.Windows. Confira o marcador CAMPAIGN_BUILD Succeeded e o arquivo produzido. O processo Android usa Library/JavaTmp para contornar a falha local do Java/Gradle. O método mantém o pacote da campanha, sem gerar outra aplicação. APK é uma build de desenvolvimento para testes privados; não é um pacote de publicação da PlayStore.
