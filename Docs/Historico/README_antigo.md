## 0.26.0 — referências, cliques, FPS e seleção

12 referências individuais geradas para Meshy e catálogo de 122 entradas futuras. Cidade/costa: clique no chão separado de colisores; 112 props da costa corrigidos e NavMeshes atualizados. F8 mostra FPS/tempo de quadro/memória; alvo sob o mouse recebe contorno. Primeiro vestígio tecnológico provisório na cidade e nova base narrativa registrada, sem reescrever a missão legada. 50/50 testes passaram; Windows compilado sem erros. Sem APK novo. [Relatório](Docs/REFERENCIAS_E_CONTROLES_0_26_0.md).

## 0.25.1 — interações consistentes na cidade

Indicador e F usam o mesmo morador próximo; clique acompanha a posição dos moradores móveis. 25/25 testes de cidade e controles passaram. APK 0.25.0 enviado para Mensagens para mim no WhatsApp; instalação pelo usuário não confirmada. [Detalhes](Docs/INTERACOES_0_25_1.md).

## 0.25.0 — cidade segura e controles de mouse (30/10/2026)

Refúgio do Último Sino separado da costa, oito NPCs provisórios, fonte funcional, treino, saída para a costa e ponto de chegada circular. PC sem WASD/F8; ataque parado sem perseguir e clique durante recuperação preservado. HUD com retrato/vida/super reais e ações circulares. 59 testes passaram; após corrigir colisão invisível do cilindro, 22 testes de cidade/controles repetidos com sucesso. Windows compilado sem erros. APK 0.25.0/code49 gerado a pedido posterior; instalação ainda não confirmada por queda do USB. Sem teste físico desta versão. [Relatório e limitações](Docs/CIDADE_E_CONTROLES_0_25_0.md).

# Atualização 0.24.1 — ataque parado e Android atual

PC: clique/segure em inimigo ou objeto quebrável para atacar sem persegui-lo; fora de alcance não aproxima. Shift + ataque dispara livremente parado. Clique no chão anda; WASD sozinho não libera tiro no chão. Regras aplicadas nos dois modos F8.

Windows e APK atuais em Builds/Atual. Android0.24.1/code48 instalado no moto g15, mesmo aplicativo/dados. Controles de toque e diálogo conferidos; perfis móveis reduzidos para melhorar desempenho (~16,5→24,6FPS em amostra curta do refúgio, sem promessa de30FPS sustentados). 54/54 testes finais, builds sem erros. [Detalhes e limitações](Docs/CONTROLES_0_24_1.md).

---

# Atualização 0.24.0 — primeiro trecho aberto do Ato 1

No Windows, escolha **COSTA DAS RUÍNAS · HISTÓRIA DO ATO 1** no menu. Região local de 320 × 320 m, refúgio central, caminhos alternativos, cais/pedreira/jardim, árvore com caveiras, dois NPCs geométricos e missão com diário persistente. F: conversar/investigar; duas pistas em qualquer ordem e retorno a Daro liberam a prévia das catacumbas. As cenas anteriores continuam disponíveis.

Esta revisão atualiza somente o PC em `Builds/Atual/Windows/ForjaDasMaresCampanha.exe`. Não foi gerado APK 0.24.0; o arquivo Android existente é de uma versão anterior. Região pública online de 20 jogadores, restante do Ato 1 e chefes narrativos ainda não estão implementados. [Relatório, instruções, testes e limites](Docs/ATO_1_0_24_0.md).

---

# Atualização 0.15.0 — Prisma, o Arcanista

Modelo original do mago com 25 clipes reais, manto/rig, animações de cajado, grimório e orbe, efeitos/sons e encaixes de arma configurados nos dados. Mantidos ataque segurado, pesado, habilidades, Convergência, equipamentos/talentos, arenas e jogo cooperativo. Corrigidos perfil sonoro por herói, indicador do super e relógio da animação de esquiva remota. Foco de Brasa preserva sua identidade.

**Testar:** use o atalho habitual em Jogos e escolha a terceira classe, ARCANISTA — Prisma. Segure ataque; E/habilidade; Espaço/esquiva; Q/super cheio; I/Personagem. Builds atuais: Builds/Atual/Windows/ForjaDasMaresCampanha.exe e Builds/Atual/Android/ForjaDasMaresCampanha.apk (0.15.0/código31). Mesmo pacote Android, protocolo10 e saves v2; não desinstalar para atualizar.

57/57 testes finais focados; duas instâncias Windows reais pelo Relay conferidas. Consolidação por caso: 231 aprovados/0 falhas/1 ignorado, não uma única execução geral final. Builds Windows e Android reais, errors0/exit0. Teste físico Android desta versão pendente: celular ausente no USB. Arte/equilíbrio ainda de protótipo. [Relatório, arquivos e limitações](Docs/PRISMA_0_15_0.md). Histórico abaixo.

---

# Atualização 0.14.2 — HUD mais limpo

Retirados os botões fixos AQUI/ALVO/PERIGO/SAQUE do PC e celular. APK atual: Builds/Atual/Android/ForjaDasMaresCampanha.apk (0.14.2/código30); envie como Documento no WhatsApp. PC: atalho existente Jogar Campanha Forja das Mares. Protocolo10/savev2 preservados; atualizar por cima, sem apagar dados.

24/24 testes focados aprovados; builds Windows e Android reais. [Relatório desta correção](Docs/REVISAO_0_14_2.md). Histórico abaixo.

---

# Revisão atual 0.14.1 — 21/10/2026

Base funcional dos 20 pontos integrada anteriormente, agora com marcações separadas, diagnóstico de entrada corrigido e proteção do movimento contra frames longos/quedas acidentais. Suíte geral:218 aprovados,0 falhas,1 ignorado (219 casos); reexecução final de segurança/interface:25/25. Quatro clientes Windows pelo Relay passaram na conferência de mapa, bênçãos, marcações, piso sem resgates, baú único, resultado administrativo e encerramento do host. Não foi teste com quatro pessoas.

Builds reais 0.14.1: Builds/Atual/Windows/ForjaDasMaresCampanha.exe e Builds/Atual/Android/ForjaDasMaresCampanha.apk (código 29). Use o atalho existente Jogos/Jogar Campanha Forja das Mares. Mesmo pacote Android e saves preservados; moto g15 atualizado e conferido com o PC pelo Relay no mesmo Wi-Fi, incluindo joystick, marcações e reconexão. Sem nova pasta de envio ao irmão.

[Lista de teste do jogador](Docs/TESTE_RAPIDO_JOGADOR_0_14_1.md) · [Relatório atual e limitações](Docs/REVISAO_0_14_1_2026-10-21.md) · [evidências](Docs/Evidencias_0_14_1/testes.json). Aprovado com limitações para testes privados no PC e neste Android; arte, equilíbrio humano, medições móveis e lançamento comercial pendentes. Histórico abaixo.

---

# Revisão atual 0.14.0 — testes privados

Arsenal original com ícones e12 novos clipes de ataque da Brasa; slots de equipamento com imagens; efeitos e sinergias de armas; esquiva perfeita; altares com escolhas; esconderijos; desafios de ondas/proteção/escolta; barris, parede quebrável e ponte frágil; marcações cooperativas e novos sons originais. A base dos20 pontos aprovados está integrada; modelos de cenário e heróis ainda são provisórios, e os demais heróis não receberam animações finais novas.

**Abrir:** Jogos/Jogar Campanha Forja das Mares. Saídas estáveis: Builds/Atual/Windows/ForjaDasMaresCampanha.exe e Builds/Atual/Android/ForjaDasMaresCampanha.apk. Android usa o mesmo pacote com.forjadasmares.campaign (versão28). Protocolo10 exige todos atualizados. Saves preservados; não apagar a instalação atual. A pasta antiga de envio ao irmão não foi atualizada.

Validação:69/69 testes na reexecução final, após corrigir13 falhas de validação detectadas na suíte geral. União do resultado mais recente por caso:214 aprovados/1 ignorado, não uma única execução geral final. Dois processos Windows pelo Relay conferidos, incluindo bênçãos, marcações, retorno, baú único e encerramento do host. Vitória de diagnóstico foi administrativa. Build Windows e APK Android reais; teste físico desta revisão pendente porque o moto g15 não está no USB. Não testado entre redes diferentes ou por quatro pessoas.

[Como jogar](Docs/CAMPANHA_COMO_JOGAR.md) · [revisão e limitações](Docs/REVISAO_0_14_2026-10-20.md) · [arquivos](Docs/ARQUIVOS_0_14.txt) · [evidências](Docs/Evidencias_0_14/testes.json).

Aprovado com limitações para testes privados. Sem conta global, migração de host ou servidor dedicado. Arte final, balanceamento humano prolongado, outros dispositivos e medições de desempenho continuam pendentes. Histórico abaixo.

---

# Revisão atual 0.12.0 — Windows validado com limitações

Mochila sem equipamentos duplicados na grade, atributos em cartões, talentos por nível, comparação individual de itens, reciclagem por ouro, drops imediatos, retrato e minimapa compacto. Consulte [instruções da campanha](Docs/CAMPANHA_COMO_JOGAR.md) e [revisão técnica](Docs/REVISAO_0_12_2026-10-18.md).

Protocolo 8 exige clientes da mesma versão. O APK anterior 0.11/protocolo 7 e a pasta de envio ao irmão permanecem preservados. Android físico 0.12 pendente; sem nova distribuição nesta rodada. Histórico abaixo descreve entregas anteriores.

---

# Versão de testes atual: 0.11.0 — 18/10/2026

PC: **Jogos/Jogar Campanha Forja das Mares**. Android: **Jogos/Enviar para meu irmao/ForjaDasMares-0.11.0-Android.apk**. Envie como Documento no WhatsApp; ambos precisam desta versão.

Minimapa com inimigos, setor pendente e objetivos; reforços intermediários sem apagar remanescentes; emissores de flechas e avisos de chegada; arma inicial comum focada, sem bônus solo; armas por classe com combos/habilidades reais; inventário por slots/categorias, secundária permitida e comparação; personagem acessível no início; barra de habilidades no PC com ícones e Fúria temporária do guerreiro. Saves mantidos e loadouts antigos incompatíveis desequipados para a mochila. Sem modelos/clips finais novos nesta revisão.

Validação: regressão PlayMode geral com 174 casos, 173 aprovados, zero falhas, 1 ignorado; grupo focado de correções: 14/14 aprovados. Após o último ajuste visual da réplica, repetidos todos os testes de rede: 65 casos, 64 aprovados, 0 falhas, 1 ignorado. Unity 6.3 LTS (6000.3.26f1). Builds reais Windows/Android 0.11.0, protocolo 7, APK ARM64 versão 25. Teste físico PC + moto g15 no mesmo Wi-Fi pelo Relay: entrada, movimento, mapa compartilhado, derrota→lobby→fase seguinte→reentrada, intenção offline validada no host e encerramento sincronizado. Resultado/recompensas na rodada anterior foram acionados administrativamente, não por vitória humana. Não testado entre redes diferentes, quatro pessoas ou FPS medido.

[Como jogar](Docs/CAMPANHA_COMO_JOGAR.md) · [relatório, builds e limitações](Docs/REVISAO_0_11_2026-10-18.md) · [arquivos](Docs/ARQUIVOS_0_11.txt).

**Aprovado com limitações para testes privados.** Host ainda encerra a sala quando sai; sem migração/servidor dedicado. Perfil deste aparelho e cache da última sala são contextos separados, não uma conta global. Arte, animações, equilíbrio humano, sessões prolongadas e desempenho em outros dispositivos ainda exigem trabalho. Não é release comercial/PlayStore.

Os blocos abaixo são históricos.

---

# Versão de testes atual:0.10.1 (17/10/2026)

Windows e Android, protocolo6. Abra **Jogos/Jogar Campanha Forja das Mares**. Para enviar ao irmão: **Jogos/Enviar para meu irmao/ForjaDasMares-0.10.1-Android.apk**, como Documento no WhatsApp. Ambos precisam da mesma versão;0.9.x é incompatível com esta revisão.

Campanha com setores maiores28/30/32m, passagens abertas, hordas com mais de100inimigos acumulados, três elites, inventário com imagens/comparação e saque equipável durante a fase fora de perigo. Sequências de três ataques nas armas e carregado/SUPER nos quatro heróis. Suprimentos curam um aliado ferido próximo uma vez.17modelos originais,12ícones e4sons, com fonte Blender/galeria; ainda são recursos provisórios.

Validação:160casos PlayMode,159aprovados,0falhas,1ignorado;16testes focados aprovados. Builds reais Windows/Android0.10.1. Teste físico PC+moto g15 pelo Relay no mesmo Wi-Fi, inventário por toque, arma equipada em escala corrigida, movimento, combate/derrota/encerramento. A sessão4/4 anterior da0.10.0 usou3processosWindows+1Android, não quatro pessoas/dispositivos. Sem medição deFPS ou teste entre redes diferentes.

[Como jogar](Docs/CAMPANHA_COMO_JOGAR.md) · [relatório e limitações](Docs/PAISAGENS_E_HORDAS_0_10_2026-10-17.md) · [assets produzidos e lista restante](Docs/ASSETS_E_DIRECAO_0_10.md) · [arquivos alterados](Docs/ARQUIVOS_0_10.txt).

**Aprovado com limitações para testes privados**, não lançamento comercial. Permanecem importantes: balanceamento humano, desempenho, arte/animações finais, mapas ainda modulares e apenas um encontro ativo por vez. Sem migração de host ou servidor dedicado. Nenhum save existente foi apagado. Os blocos abaixo são históricos.

---

# Versão de testes atual: 0.9.1 (17/10/2026)

Windows e Android, protocolo 5. Abra o atalho **Jogar Campanha Forja das Mares** na pasta Jogos da Área de Trabalho. Para enviar ao irmão, use **ForjaDasMares-0.9.1-Android.apk**, na pasta **Jogos/Enviar para meu irmao**, como Documento no WhatsApp. Ambos devem usar a mesma versão; builds 0.8.x são incompatíveis.

A campanha existente foi ampliada com suprimentos quebráveis, três selos opcionais por mapa, fontes de cura limitadas, nove variantes de encontros e três armas com padrões diferentes. Guerreiro: segure ataque para carregar; SUPER após 18 acertos em inimigos, Q no PC ou botão no Android. Os demais heróis mantêm ataques e habilidades anteriores. Nenhum inventário ou save existente foi apagado.

Detalhes técnicos, arquivos, evidências e limites: [relatório da entrega](Docs/AVENTURA_0_9_2026-10-17.md). Os blocos abaixo são históricos, não a versão para compartilhar agora.

Validação final:151 testes PlayMode,150 aprovados,0 falhas,1 ignorado; builds Windows/Android0.9.1 e teste físico PC–moto g15 pelo Relay. Sessão4/4 anterior deste bloco:3 processosWindows+1Android, não quatro pessoas.

---

## Campanha 0.8.2 — HUD e convite em jogo

Código visível, menu de sessão e APK para testes privados: [relatório](Docs/HUD_E_COMPARTILHAMENTO_2026-10-17.md). Builds em `Builds/CampaignWindows-0.8.2` e `Builds/CampaignAndroid-0.8.2`; binários locais não estão versionados.

﻿# Forja das Marés — protótipo de ação e masmorras

Unity 6.3 LTS 6000.3.26f1, URP, C#, Input System e AI Navigation 2.0.16. Plataformas atuais: Windows e Android; iOS adiado. VS Code é o editor de código; Visual Studio IDE não é necessário.

## Campanha — revisão0.8.1

Início solo em todos os mapas, bônus temporário de10% em vida/dano, placas sequenciais no solo e sala privada que o host pode abrir para grupos. Novas pessoas entram na mesma fase até antes de20% de setores concluídos, inclusive com a partida iniciada. Tela inicial com campanha, agenda local de convites, volume e saída confirmada. Relatório atual: [campanha solo e entrada em andamento](Docs/CAMPANHA_SOLO_2026-10-17.md); [instruções atualizadas](Docs/CAMPANHA_COMO_JOGAR.md).

Três fases de12/15/18 setores,1–4 participantes por tentativa (incluindo reservas). Placas simultâneas em grupo, flechas com aviso, névoa exterior e mochila inicial vazia com equipamentos ao redor da prévia3D. O grupo pode voltar ao mapa após o resultado e iniciar outra fase sem trocar de código. Saves antigos preservados; progresso fica no host. Não há conta global ou lista de amigos autenticados: a agenda guarda códigos compartilhados neste aparelho.

Windows: `Builds/CampaignWindows-0.8.1/ForjaDasMaresCampanha.exe`; Android: `Builds/CampaignAndroid-0.8.1/ForjaDasMaresCampanha.apk`; cena `Assets/ForjaDasMares/Scenes/CampaignCoop.unity`. Use o atalho Jogar Campanha Forja das Mares em Desktop/Jogos. Builds não são incluídas no Git.

Revisão0.8.1:34 casos de rede,33 aprovados/1 ignorado/0 falhas. Testes reais Windows/Relay e PC–moto g15 por Wi-Fi, com instalação0.8.1/code19, navegação por toque, movimento, mochila antes de recompensas e prévia3D. Relatório registra bugs encontrados/corrigidos e quais fluxos foram repetidos após cada alteração. Quatro dispositivos, redes distintas, vitórias humanas completas e desempenho sustentado continuam pendentes. Não é lançamento comercial nem aprovação para Play Store. Regressão ampla137/138 e testes39/39 e13/13 pertencem ao checkpoint0.8.0 anterior, preservado em Docs/CAMPANHA_2026-10-17.md; não foram apresentados como repetição integral na0.8.1.

## Mapa local Galerias — revisão0.7.2, locomoção do Brasa

Atalho Desktop/Jogos/Jogar Galerias da Mare.lnk abre Builds/GaleriasWindows-0.7.2/ForjaDasMaresGalerias.exe, compilado sem erros. Cena inicial GaleriasDungeon. APK Builds/GaleriasAndroid-0.7.2/ForjaDasMaresGalerias.apk (ARM64/code17) gerado, assinatura validada e instalado no moto g15; teste visual0.7.2 aguarda desbloqueio do aparelho. A revisão0.7.1 anterior foi executada nele. Câmera contínua e obstáculos semitransparentes, HUD rústico, mochila/equipamentos/opções existentes preservados.

Brasa possui fonte original Blender,17 ossos e13 clipes reais. Correção de cópias sobrepostas, ataque segurado, pesado e barra SUPER em0.7.1;0.7.2 separa tronco/pernas, calibra passada pela distância real, adiciona corrida e apoios com IK, além de postura parada com respiração. Não há root motion nem mudança de velocidade/collider do jogador. Testes focados finais32/32 aprovados; isso não é validação artística final ou garantia de FPS em todos os aparelhos.

Fonte ArtSource/Characters/Brasa/Brasa.blend e create_brasa.py; Blender portátil em Area Dev/Ferramentas. Relatórios: Docs/BRASA_MODELO_2026-10-16.md, BRASA_COMBATE_2026-10-16.md e BRASA_MOVIMENTO_2026-10-16.md, com arquivos/configuração/evidências/limitações. Novos visual/pesado/super são locais às Galerias; outros heróis e cooperativo conservam seus visuais atuais. Histórico completo em Docs/PROGRESSO.md. Janelas antigas precisam ser reabertas para executar a revisão nova.

## Executar

Versão local de progressão0.5.0 preservada. Cooperativo0.6.1 foi testado em Windows/Android na LAN;0.6.2 inclui vínculo Unity Cloud e salas privadas por MPS/Lobby/Relay, já conectadas realmente entre Windows e moto g15 em Wi-Fi. A versão atual é de desenvolvimento, não aprovada para Play Store. Builds e evidências constam em Docs/PROGRESSO.md e Docs/REDE_INTERNET_2026-10-15.md.

Para cooperativo, abrir Assets/ForjaDasMares/Scenes/CoopDungeon.unity. Executável vinculado: Builds/InternetWindows-0.6.5/ForjaDasMaresCoop.exe; APK: Builds/InternetAndroid-0.6.5/ForjaDasMaresCoop.apk. Host usa CRIAR SALA INTERNET; cliente informa o código retornado no campo abaixo desse botão e usa ENTRAR POR CÓDIGO. Host inicia a partida após os jogadores entrarem. Não usar IP/código de LAN para entrar no Relay. Configuração e limites: Docs/INTERNET_E_PUBLICACAO.md. Sair do host encerra a partida; sem migração.

Abra Assets/ForjaDasMares/Scenes/ProgressionDungeon.unity e pressione Play. Menu Forja das Mares → Open progression dungeon. VOLTAR A ARENA acessa ProgressionArena com equipamento/inventário; nessa arena, MASMORRA retorna ao percurso. ArenaTest e DungeonTest permanecem como referência e testes da base; F6 abre ArenaTest.

No modo local: WASD/setas, mouse para mirar/atacar, E/clique direito para habilidade, Espaço para esquiva, 1–4/Tab para herói, R reinicia. F/gamepad botão norte/INTERAGIR usa objeto próximo. No cooperativo, escolher herói no lobby; o host controla o início. Android mantém joystick e botões por toque. Mira automática, ajustes completos dos controles e menus finais pertencem à próxima etapa de interface.

Quatro heróis originais, cinco arquétipos de inimigos, invocações limitadas, três ondas de arena e chefe com três ataques/fases. Masmorras com três biomas, oito categorias de salas, seed configurável, mapa fixo, conexões físicas, portas, baús, chave opcional, descanso, objetivos, armadilhas e saída após chefe. Progressão local com9 categorias de armas,5 raridades,5 slots por conjunto de herói, inventário/comparação/descarte confirmado, entregas, XP/nível/moedas, saque temporário, resultado e save versionado. Mascote de suporte com cura/cargas finitas. Modelos geométricos e poses são provisórios: não há arte ou clips finais.

## Verificação

Revisão de câmera0.6.3: regressão97 casos,96 aprovados,0 falhas,1 ignorado; detalhes em Docs/CAMERA_CORRECAO_2026-10-15.md. Regressão geral89/89 aprovada no checkpoint anterior de integração cooperativa. Última suíte específica de rede/internet:21 casos,20 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. Quatro processos Windows/LAN e PC–Android físico testados; Relay real com combate, recompensa e retorno após encerrar processo também foi verificado. Esses testes não representam vitória humana completa com quatro jogadores, profiling sustentado ou redes/operadoras distintas. Históricos de testes locais/geração/auditoria continuam nos respectivos relatórios; consultar suas datas e limites.

Estado dos pedidos: base local implementada; multiplayer em validação privada, interface completa parcial, expansão de conteúdo e revisão de lançamento pendentes. Ver Docs/ESTADO_DOS_PEDIDOS.md. Arte e animações finais, autenticação comercial, servidor dedicado e economia online segura ainda não foram entregues.

## Compilar por plataforma

No Editor, os menus de build ativam a plataforma correspondente antes de compilar. Em modo batch, inicie a Unity com `-buildTarget Win64` para `ForjaDasMares.Editor.ArenaBuilds.Windows` ou `-buildTarget Android` para `ForjaDasMares.Editor.ArenaBuilds.Android`. O método interrompe a compilação se o destino ativo estiver errado: a URP usa esse destino para selecionar e remover recursos gráficos. Não basta definir somente BuildPlayerOptions.target após alternar entre Android e Windows.

## Guias de conteúdo

- Docs/MASMORRAS_COMO_JOGAR.md
- Docs/PROGRESSAO_COMO_JOGAR.md
- Docs/PROGRESSAO_ARQUITETURA.md
- Docs/REVISAO_TECNICA_2026-10-14.md
- Docs/MASMORRAS_ARQUITETURA.md
- Docs/PVE_COMO_JOGAR.md
- Docs/HEROIS_COMO_TESTAR.md
- Docs/PROGRESSO.md
- Docs/COOP_COMO_TESTAR.md
- Docs/INTERNET_E_PUBLICACAO.md
- Docs/REDE_INTERNET_2026-10-15.md
- Docs/ESTADO_DOS_PEDIDOS.md
- Docs/ARQUITETURA.md (plano mestre de longo prazo)

Backup contém Assets e .meta, Packages, ProjectSettings, código e documentação. Library, Temp, Logs e Builds ficam fora do Git. Para recuperar, clone/baixe o repositório e abra a pasta inteira na versão de Unity indicada, aguardando a resolução de pacotes. Não recupere somente os scripts, pois cenas/materials/configurações também são necessários.



Câmera atual0.6.5: obstáculos semitransparentes35%, sete testes específicos aprovados; builds PC/Android geradas, Android físico desta revisão pendente. Ver Docs/CAMERA_TRANSPARENCIA_2026-10-15.md.

## Mapa de estilo — Galerias da Maré

Variante local original0.6.6 em Assets/ForjaDasMares/Scenes/GaleriasDungeon.unity, com GaleriasArena para retorno. PC: Builds/GaleriasWindows-0.6.6/ForjaDasMaresGalerias.exe, atalho Jogar Galerias da Mare em Jogos. Perfil separado; mapa online anterior preservado. Peças geométricas provisórias salvas e substituíveis, combate/baús/chefe/progressão reais existentes. Não contém destruição de cenário nem integração online deste mapa. Testes, configuração e limites em Docs/MAPA_GALERIAS_2026-10-15.md.

APK local separado deste mapa: Builds/GaleriasAndroid-0.6.6/ForjaDasMaresGalerias.apk (com.forjadasmares.galerias), produzido/assinado, sem teste físico desta revisão. Para compilar, usar GaleriasInstaller.Windows com Win64 ativo ou GaleriasInstaller.Android com Android ativo. Esses métodos mantêm o mapa online anterior separado.
