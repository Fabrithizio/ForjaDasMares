# Jogar a campanha 0.9.1

## PC e Android juntos

1. PC: abra **Jogos/Jogar Campanha Forja das Mares**. Android: instale **Jogos/Enviar para meu irmao/ForjaDasMares-0.9.1-Android.apk**. Para compartilhar, envie esse APK como Documento no WhatsApp; não envie só o executável de PC.
2. Ambos precisam de internet e da mesma versão. Host: CAMPANHA → fase → CRIAR SALA PRIVADA. Compartilhe o código exibido.
3. Convidado: CAMPANHA → informe o código → ENTRAR PELO CÓDIGO. Host: INICIAR COM O GRUPO. Prefira aguardar todos no lobby.
4. Pode iniciar sozinho (+10% vida/dano enquanto estiver só). Uma sala publicada recebe jogadores até 20% do percurso e no máximo quatro; uma sala privada depende de código.
5. MENU/Esc abre sala, convites, mapa e opções durante a fase. Não pausa o online. Inventário possui equipamento, comparação e descarte confirmado; o resultado mostra os itens obtidos.
6. Mantenha o host aberto; sair encerra a sessão. O host guarda a progressão cooperativa. Não há migração de host nem transferência automática de progresso entre hosts. Amigos são agenda local de convites, não uma rede social com presença.

## Novidades para experimentar

- Quebre os suprimentos dos caminhos laterais para obter pequenas recompensas.
- Encontre e destrua os três selos: equipamento raro ou melhor, XP e moedas, uma vez por expedição.
- INTERAGIR perto de uma fonte recupera 35% da vida máxima dos aliados vivos no mesmo setor, fora de combate; cada fonte é de uso único.
- Guerreiro: segure ataque para carregar o golpe pesado (1,2s; intervalo 4s). SUPER enche após 18 acertos válidos; Q no PC/botão SUPER no toque.
- Corta-Maré, Salva de Junco e Foco de Brasa mudam arco/alcance, disparos em leque e explosão, respectivamente. Os visuais ainda são provisórios.
- MAPA E OBJETIVOS mostra os setores reais, sua posição e progresso; o HUD informa cooldowns e personagem caído.

Cena do Editor: `Assets/ForjaDasMares/Scenes/CampaignCoop.unity`. Pressione Play após terminar a importação. As cenas de arena e masmorras anteriores foram preservadas.

Builds: `Builds/CampaignWindows-0.9.1/ForjaDasMaresCampanha.exe` e `Builds/CampaignAndroid-0.9.1/ForjaDasMaresCampanha.apk`. APK ARM64, Android mínimo API25, assinado para desenvolvimento; não é release da Play Store. Atualize por cima para preservar os dados. O Android pode pedir autorização pessoal para instalar o arquivo recebido.

Ainda exige sessões humanas completas para balancear dificuldade/duração, outros modelos de celular e testes entre redes diferentes. Esta é uma versão privada de testes, com arte provisória em parte do conteúdo. Evidências e dependências: `AVENTURA_0_9_2026-10-17.md`.

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
