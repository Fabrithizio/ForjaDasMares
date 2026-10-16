# Forja das Marés — protótipo de ação e masmorras

Unity 6.3 LTS 6000.3.26f1, URP, C#, Input System e AI Navigation 2.0.16. Plataformas atuais: Windows e Android; iOS adiado. VS Code é o editor de código; Visual Studio IDE não é necessário.

## Mapa local Galerias — revisão0.7.0, personagem Brasa

Atalho Desktop/Jogos/Jogar Galerias da Mare.lnk abre Builds/GaleriasWindows-0.7.0/ForjaDasMaresGalerias.exe. APK separado Builds/GaleriasAndroid-0.7.0/ForjaDasMaresGalerias.apk instalado/testado no moto g15. Cena inicial GaleriasDungeon; câmera acompanha continuamente entre encontros e mantém obstáculos semitransparentes. HUD rústico com vida/XP, objetivo, mochila/menu compactos, ícones originais, botões circulares e cooldowns no Android; dock com atalhos no PC. Ferramentas de seed/bioma/reset ficam no menu. Inventário/equipamentos/resultado/opções preservam as funções existentes.

O guerreiro local usa agora o Brasa: primeiro modelo 3D autoral, rig de17 ossos e8 clipes básicos reais. Fonte editável em ArtSource/Characters/Brasa/Brasa.blend; atalho Jogos/Editar personagem Brasa abre o Blender portátil instalado em Area Dev/Ferramentas. Prévia Brasa_preview.png e gerador create_brasa.py acompanham a fonte. Outros heróis e cenas online conservam seus modelos atuais. Arte/animação ainda exigem acabamento, revisão dos pesos/poses e otimização dos13 materiais. Relatório e arquivos: Docs/BRASA_MODELO_2026-10-16.md.

Validação0.7.0: Windows compilado sem erros e aberto; Android ARM64/code15 gerado por Gradle direto após falha local do Java no empacotador interno, instalado e executado. Testes focados26/26 aprovados; a rodada ampla anterior teve112 aprovados,1 falha de tempo no percurso cooperativo e1 ignorado. O mesmo percurso passou na repetição focada; a suíte inteira não foi repetida. Não houve teste online deste modelo ou profiling sustentado.

Esta variante é local. O lobby online continua com apresentação própria e builds anteriores abaixo. Janelas já abertas conservam a versão antiga até reabrir o novo atalho. Tema definitivo, arte final e profiling continuam pendentes. Relatórios: Docs/HUD_RUSTICO_2026-10-16.md, Docs/CAMERA_CONTINUA_2026-10-15.md e Docs/MAPA_GALERIAS_2026-10-15.md.

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
