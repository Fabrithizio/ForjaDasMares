# Forja das Marés — protótipo de ação e masmorras

Unity 6.3 LTS 6000.3.26f1, URP, C#, Input System e AI Navigation 2.0.16. Plataformas atuais: Windows e Android; iOS adiado. VS Code é o editor de código; Visual Studio IDE não é necessário.

## Campanha online — revisão0.8.0

**Revisão seguinte0.8.1:** início solo em todos os mapas, bônus temporário de10% em vida/dano, placas sequenciais no solo e sala privada que o host pode abrir para grupos. Novas pessoas entram na mesma fase até antes de20% de setores concluídos, inclusive com a partida iniciada. Tela inicial com campanha, agenda local de convites, volume e saída confirmada. Relatório atual: [campanha solo e entrada em andamento](Docs/CAMPANHA_SOLO_2026-10-17.md); [instruções atualizadas](Docs/CAMPANHA_COMO_JOGAR.md). Caminhos0.8.0 e evidências abaixo são históricos, preservados; use o atalho atualizado para0.8.1 quando a validação final estiver registrada.

Mapa com três fases de12/15/18 setores, grupo de2–4, sala privada por código e busca de sala pública aguardando parceiros na mesma fase. Placas simultâneas para dois heróis, flechas com aviso, névoa exterior e mochila inicial vazia com equipamentos ao redor da prévia3D. O grupo pode voltar ao mapa após o resultado e iniciar outra fase sem trocar de código. Saves antigos preservados; progresso da campanha fica no host.

Windows: `Builds/CampaignWindows-0.8.0/ForjaDasMaresCampanha.exe`; cena `Assets/ForjaDasMares/Scenes/CampaignCoop.unity`. Consulte [como jogar em dupla](Docs/CAMPANHA_COMO_JOGAR.md) e [relatório de implementação e limites](Docs/CAMPANHA_2026-10-17.md). Builds não são incluídas no Git.

Compilação Windows sem erros; regressão ampla137 aprovados/1 ignorado/0 falhas, depois39/39 e13/13 testes focados. Descoberta pública, combate, baú sem duplicação, resgate, reconexão e troca de fases exercitados entre dois processos Windows reais pelo Relay. Conclusões de teste foram administrativas, sem alegar vitória humana completa. Validação física da campanha Android, quatro jogadores e redes distintas ainda pendentes. Esta revisão não é lançamento comercial nem aprovação para Play Store; as seções abaixo documentam variantes anteriores preservadas.

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
