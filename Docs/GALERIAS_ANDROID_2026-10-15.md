# Galerias da Maré — verificação física no Android

## Resultado

Aprovado com limitações para equivalência do mapa local de teste. Em 15/10/2026, instalado por ADB e executado realmente no moto g15 conectado. APK existente 0.6.6/code12, pacote separado `com.forjadasmares.galerias`, preservando o aplicativo online `com.forjadasmares.prototype`. Nenhuma alteração em código, cenas ou dados foi necessária nesta verificação.

## Evidências observadas

- Inicialização direta no mapa com título Galerias da Maré, seed73129, modo fixo. Log do processo confirma `DUNGEON_RUNTIME_READY biome=galerias-da-mare seed=73129`.
- Piso modular, paredes de alvenaria, lanternas iluminadas, caixas e grades renderizados, correspondendo aos mesmos assets e disposição da versão Windows. Personagem Guerreiro/Brasa e mascote visíveis.
- Joystick acionado por gestos: deslocamento da entrada pelo corredor até a primeira sala de combate. Acompanhamento da câmera observado nas capturas.
- Inimigos apareceram, portas fecharam durante o encontro. Toques em habilidade e ataque derrotaram o grupo; sala mudou para Concluída e grades abriram. HUD registrou +39XP/8 moedas na expedição. Jogador recebeu dano, chegando a58/140, e permaneceu vivo. Não se trata de teste completo da masmorra.
- Aproximação da parede frontal mostrou alvenaria semitransparente e personagem visível através dela. Repetir movimento em direção à parede manteve o personagem contido; não atravessou o limite. Afastar-se restaurou a parede opaca.
- Botão Esquiva acionado isoladamente depois de caminhar: capturas antes/depois mostram deslocamento adicional sem gesto no joystick. Não medidos frames de invulnerabilidade neste aparelho.
- Tela física em paisagem2400×1080. Enquadramento lateral mais amplo que PC1280×720, esperado pela proporção; não se promete igualdade de pixels. Controles acessíveis no aparelho, porém HUD de protótipo ocupa espaço excessivo e mantém legendas de atalhos de PC.
- Logcat restrito ao PID do jogo examinado: nenhuma exceção Unity, referência ausente ou falha fatal encontrada no trecho desta execução. A palavra error em configuração de callback do áudio não era erro de execução.

## Evidências locais

Workspace de QA: `C:/Users/Fabricio_S/Documents/Codex/2026-10-12/f/work/galerias/`.

Capturas `android-initial.png`, `android-moved.png`, `android-corridor.png`, `android-combat.png`, `android-wall.png`, `android-collision.png`, `android-before-dodge.png`, `android-after-dodge.png`; log `android-device.log`. Arquivos de evidência locais não incluídos no repositório. APK real em `Builds/GaleriasAndroid-0.6.6/ForjaDasMaresGalerias.apk` (builds ignoradas pelo Git).

## Limites e pendências

Teste por toques/gestos ADB em dispositivo real, não sessão humana completa. Não medidos FPS sustentado, temperatura, consumo, latência de entrada ou suavidade quadro a quadro. Não testados todos os heróis, chefe, baús, resultado final ou multiplayer neste recorte. Mapa Galerias continua variante local; integração ao mapa online continua pendente. Verificação visual não substitui a avaliação do usuário jogando. Nenhuma nova compilação executada, pois foi instalado o APK já produzido e verificado na entrega anterior.

## Arquivos desta entrega

Criado este relatório; atualizado `Docs/PROGRESSO.md` e adendo em `Docs/MAPA_GALERIAS_2026-10-15.md`. Jogo deixado aberto em sala de combate já concluída, sem inimigos ativos naquela sala, para teste do titular.
