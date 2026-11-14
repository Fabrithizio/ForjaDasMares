# Som ambiente e trilhas — 0.19.0

Implementado na campanha: cinco trilhas originais (menu, catacumbas, floresta subterrânea, fortaleza vulcânica e chefe), quatro ambientes contínuos e vinhetas de partida, vitória e derrota. Composição e síntese próprias, sem amostras externas. Não são gravações de instrumentos reais.

Transições de 2,5 segundos, cinco AudioSources limitados por sessão local. Música e ambiente têm controles separados em Configurações no início e no menu da partida; volume geral e efeitos preservados. Preferências locais persistidas. Loops Vorbis por streaming; vinhetas descomprimidas no carregamento. Biblioteca real em Resources/CampaignSoundscape.asset; não há configuração manual pendente na campanha. Cenas antigas de referência não receberam integração separada.

## Verificação

26/26 testes focados passaram (SoundscapeTests e CampaignTests). Três pressupostos antigos de testes foram atualizados: seleção explícita de guerreiro e protocolo já existente 11; nenhuma alteração de protocolo ou balanceamento.
Build Windows 0.19.0 concluída, errors=0. Execução real do PC em perfil isolado confirmou MusicPlaying e AmbiencePlaying verdadeiros, saída 48 kHz e avanço de musicSample de 50517 para 1209002. Sem exceções de áudio identificadas no log dessa execução. Isso verifica reprodução técnica, não substitui avaliação auditiva do usuário.
12 WAVs estéreo conferidos por leitura PCM, sem clipping; nove loops com amostras inicial/final iguais. Trilhas 34–48 s, ambientes 40 s, vinhetas 3 s. Limitação: repetição perceptível em sessões longas pode exigir mais variações musicais.
Sem APK, teste físico Android ou novo teste multiplayer nesta entrega. Fontes mantêm compatibilidade móvel; custo real em celular ainda não medido.

## Como testar

Abra Builds/Atual/Windows/ForjaDasMaresCampanha.exe. Ouça o menu; em Configurações altere Música e Ambiente separadamente. Inicie fases dos três biomas e chegue ao chefe para ouvir mudanças. Confira vitória/derrota e retorno ao menu. Avalie se combate permanece audível e ajuste os volumes conforme preferência.

## Arquivos

Novos: ArtSource/Audio/create_soundscapes.py e soundscape_report.json; Audio/Soundscapes com 12 WAVs; CampaignSoundscape.cs; SoundscapeLibrary.cs; Editor/SoundscapeInstaller.cs; Tests/SoundscapeTests.cs; Resources/CampaignSoundscape.asset (e respectivos metas).
Alterados: CoopDirector.cs, CampaignHome.cs, CampaignSessionMenu.cs, CoopSmokeProbe.cs (diagnóstico), Editor/CampaignInstaller.cs, Tests/CampaignTests.cs, ProjectSettings/ProjectSettings.asset, Docs/PROGRESSO.md.
Gerador requer Python e NumPy; execute ArtSource/Audio/create_soundscapes.py. Reimportação/configuração: método Editor SoundscapeInstaller.Apply. Logs locais: soundscape-tests.xml, soundscape-windows.log, soundscape-player-qa.log.
