# Campanha cooperativa 0.8.0 — teste em dupla

## Abrir

PC: abra o atalho **Jogar Campanha Forja das Mares** na pasta Jogos da Área de Trabalho, ou execute `Builds/CampaignWindows-0.8.0/ForjaDasMaresCampanha.exe`. Cena do Editor: `Assets/ForjaDasMares/Scenes/CampaignCoop.unity`. Os dois jogadores precisam usar a versão0.8.0 da campanha; versões antigas não entram nesta sessão.

Android: APK gerado em `Builds/CampaignAndroid-0.8.0/ForjaDasMaresCampanha.apk`. É um aplicativo separado das Galerias, para Android ARM64, mínimo API25. Instalação e teste físico desta revisão ainda pendentes porque o aparelho desconectou. Não é um pacote aprovado para a Play Store. Não compartilhe somente o executável do Windows: a pasta de build inteira é necessária.

## Você e seu irmão

1. Ambos abrem a campanha com internet.
2. Quem vai hospedar escolhe a fase no mapa e usa CRIAR SALA PRIVADA.
3. Envie ao parceiro o código mostrado (COPIAR CÓDIGO copia para a área de transferência).
4. O parceiro informa esse código e usa ENTRAR POR CÓDIGO.
5. Cada um escolhe seu herói. Com pelo menos dois conectados, o host inicia.
6. Após o resultado, o host usa CONTINUAR CAMPANHA, escolhe outra fase e inicia novamente. O grupo e o código são preservados.

O Relay dispensa informar IP local ou abrir portas no roteador. Isso não garante funcionamento em toda rede: falhas de conexão precisam ser investigadas com os logs. Se o Windows apresentar um aviso de Firewall, responda pessoalmente conforme a rede em que confia.

## Encontrar parceiros

Escolha a fase e use PROCURAR GRUPO DESTA FASE. O jogo procura uma sala pública aguardando jogadores nessa fase; se nenhuma estiver disponível, cria uma e espera alguém. Não existem jogadores automáticos. O host precisa iniciar com2–4 participantes. Novos jogadores não entram no meio de uma fase já iniciada; reconexão de participante reservado é uma operação diferente.

## Controles e desafios

- PC: WASD/setas para mover; mouse para mirar; segure ataque para repetir golpes; E para habilidade; Espaço para esquiva; F para interagir/resgatar. Use MOCHILA para equipamentos.
- Android: joystick esquerdo e botões de ataque, habilidade, esquiva e interação à direita. Mantenha ataque pressionado para repetir.
- Entrem juntos nos setores: o encontro começa quando os heróis vivos estão dentro. Derrotem as ondas para liberar as portas.
- Cada placa requer uma pessoa viva diferente. Permaneçam nas duas placas simultaneamente até completar o indicador.
- Linhas de flechas avisam antes de disparar. Baús e saída são usados com interação próxima.
- Fora de combate, aproxime-se de aliado caído e interaja para resgatá-lo. Uma parede impede o resgate.
- A mochila da campanha começa vazia; o ataque base do herói funciona sem uma arma equipada. Baús/chefes são fontes mais confiáveis de equipamento que inimigos comuns.
- Se a mochila lotar, itens excedentes ficam em ENTREGAS. Libere espaço e receba-os. Equipar respeita as restrições da classe; descarte exige confirmação.

## Salvamento e limites importantes

O progresso é guardado no PC/aparelho que hospeda, em perfis separados dos saves antigos. Não é uma conta global sincronizada entre hosts. Joguem com o mesmo host para manter essa continuidade. Fechar o host encerra a partida de todos; não existe migração de host.

As três fases estão liberadas para teste, com12,15 e18 setores. A duração desejada da primeira é cerca de5–7 minutos em dupla, ainda sem medição de uma vitória humana completa. O chefe existente e os cenários são provisórios. Brasa possui o modelo/animações autorais; as demais classes continuam com placeholders. O pesado e SUPER da variante local das Galerias ainda não são recursos desta campanha online.

Dois clientes Windows pelo Relay foram testados. A nova campanha em Android, quatro jogadores e redes distintas ainda precisam de validação real. Não confundir os testes anteriores de PC–Android da versão cooperativa antiga com esta versão0.8.0.

Detalhes técnicos, testes e arquivos: [relatório da campanha](CAMPANHA_2026-10-17.md).
