# Ambiente 0.27.0 — entrega com limitações

05/11/2026. Retomada da ordem A01/A02 do Trello, com primeiro caso de A04. A ativação/login da Unity foi concluída pelo titular; o bloqueio de licença descrito no relatório anterior foi resolvido.

## O que entrou de verdade

- 36 prefabs originais, cada um com LOD0/LOD1 e material URP de paleta compartilhada. Fontes editáveis em ArtSource/SettlementKit; FBX/prefabs em Assets/ForjaDasMares/Art/World/SettlementKit.
- Cidade com 87 instâncias do kit, mais a nova fonte: três casas, bancas, bancos, carrinho, cesto, lenha, estante, cercas, luminárias, árvores, arbustos folhosos, floreiras, samambaias e flores. As casas agora têm portas, janelas, vigas, telhas e degraus modelados. Fachadas corrigidas para a aproximação central após conferência visual na build.
- Duas rotas existentes da costa receberam árvores, vegetação e sinalização. O terreno, posições dos acampamentos, alternativas de percurso, combate e missão não foram recriados.
- Fonte conserva a cura real e ganhou pulso visual ao interagir. Componente reutilizável WorldInteractionFeedback, sem recompensas/dano e sem listeners duplicados ao reativar. Não significa que todas as interações do jogo já tenham feedback completo.
- Colisões simples distintas da copa e dados de navegação atualizados nas duas cenas. Instalador recusa destinos desconectados antes de substituir o asset estável de navegação.

## Verificações executadas

1. Instalação no Editor 6000.3.26f1: `settlement027-install-login.log`, `SETTLEMENT_READY prefabs=36 scenes=2 storyPreserved=true`, saída 0, sem erros C#.
2. PlayMode: **53/53 passaram**, zero falhas/ignorados, em `Logs/settlement027-tests.xml`. Conjunto TownArtTests, TownTests, ActOneTests, ArpgControlsTests e InventoryClarityTests.
3. Após corrigir orientação das casas: **12/12 checks da cidade repetidos e aprovados**, `Logs/settlement027-town-final.xml`. São parte dos testes anteriores, não 12 testes únicos adicionais.
4. Windows final: `Logs/settlement027-windows-final.log`, `CAMPAIGN_BUILD StandaloneWindows64 Succeeded errors=0`, saída 0. Build real aberta, casas/fonte/mercado conferidos por captura. Clique no chão moveu a personagem na build; relatório do probe registrou mudança de posição.
5. Android: `Logs/settlement027-android.log`, `CAMPAIGN_BUILD Android Succeeded errors=0`, saída 0. APK 0.27.0 / código 50, ARM64, atualizado por `adb install -r` no moto g15 sobre 0.25.0 / código 49. Resultado Success; pacote/versionName/versionCode confirmados. Um único pacote com.forjadasmares.campaign, sem desinstalar nem limpar dados.
6. No aparelho real: abriu menu e cidade; joystick por gesto moveu a personagem; ataque segurado produziu projétil visível; casas/vegetação e controles renderizaram. O diário concluído da versão anterior continuou aparecendo após a atualização. Sem exceção fatal/NullReference/erro FMOD no trecho de logcat consultado. Não houve escuta/medição acústica do som.

## Desempenho e limites das amostras

PC: capturas curtas sem combate mostraram cerca de 76–83 FPS na GeForce MX130 a 1280x800. Outra abertura ocorreu enquanto compilava Android, com contenção de recursos; não é comparação controlada nem garantia de estabilidade prolongada.

moto g15 / Android 15 / tela 2400x1080: SurfaceFlinger, timestamps de apresentação da SurfaceView da Unity, amostra de 125 quadros na cidade: **28,84 FPS médios, p95 de 49,98 ms**. Meta de 30 FPS ainda não foi atingida de forma sustentada. Não confundir taxa de atualização de 60 Hz da tela com FPS renderizado. Sem teste prolongado de calor/bateria, hordas ou online nesta revisão.

## Áudio no PC

Na segunda abertura, o log apontou mudança do dispositivo de saída e FMOD 60; uma nova abertura também ficou sem dispositivo utilizável. O titular informou estar sem saída de som disponível. O problema fica registrado como limitação ambiental do teste; não foi ocultado nem declarado corrigido. Conferir novamente com saída de som disponível. O código de áudio do jogo não foi reescrito nesta entrega.

## Como testar

- PC: executar Builds/Atual/Windows/ForjaDasMaresCampanha.exe e escolher ENTRAR NA CIDADE. Passe pela oficina, mercado e fonte; clique no chão para andar, clique em morador para conversar ou use F próximo. Fonte restaura vida e mostra pulso; instrutora troca herói; saída sul leva à costa. F8 exibe as medições locais.
- Celular: abrir a única instalação Forja das Marés, versão 0.27.0, já atualizada. ENTRAR NA CIDADE; joystick à esquerda; ataque segurado, esquiva, especial e USAR à direita. Nenhum APK precisa ser instalado novamente para testar esta entrega.
- APK produzido para eventual cópia manual: Builds/Atual/Android/ForjaDasMaresCampanha.apk. O instalador do Android preserva os dados na atualização; não é preciso desinstalar.

## O que não foi entregue

Não é o mundo completo, a nova história inteira ou região pública de 20 jogadores. Cidade continua local. NPCs são provisórios. Casas são exteriores, sem interiores acessíveis. Materiais são estilizados de cores simples, sem acabamento PBR pintado final. Lanternas não criam luz dinâmica individual; tecidos/vegetação não têm simulação de vento.

A03 (origem e primeiro conflito), A05 (aventura nova) e A06 (segredos) continuam pendentes. A missão antiga do sino permanece como legado e seus IDs/saves não foram reescritos. A04 ainda precisa alcançar outros objetos e futuras missões, com persistência e consistência online quando aplicáveis. Multiplayer, morte/loot online e PC–Android não foram retestados nesta etapa.

## Arquivos e dependências

Novos: TownArtInstaller.cs, WorldInteractionFeedback.cs, TownArtTests.cs, kit FBX/paleta/prefabs/metas, fontes Blender e este relatório.
Alterados: RefugioUltimoSino.unity, CostaAto1.unity, seus Navigation.asset; ActOneTests.cs (aceita dois/três LODs); RegionalProbe.cs (pontos de inspeção opt-in de desenvolvimento); CampaignInstaller.cs (0.27.0, Android code 50); ProjectSettings.asset e documentação de progresso.

Nenhum pacote foi atualizado ou acrescentado. Não atualizar o Editor para outra versão para abrir esta entrega. Antes de ampliar o mapa, avaliar os novos assets no jogo e definir a função/origem dos lugares e personagens da próxima parte.
