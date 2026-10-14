# Teste físico Android — versão local0.5.0

Aparelho real: moto g15, Android15/API35, ARM64, tela1080x2400 (jogo em paisagem2400x1080). Em14/10/2026, versão instalada0.2.0 foi atualizada com adb install -r para o APK0.5.0 já produzido e assinado, preservando os dados.

Observado em capturas do aparelho: masmorra e arena renderizam; joystick move e aponta após arrasto horizontal/diagonal; ataque básico Guerreiro reduz alvo100→74; habilidade reduz74→29 e mostra cooldown5s; troca para Patrulheiro e reinício restauram alvo100; projétil à distância reduz100→80. Inventário abre por toque, comparação real mostra crítico0→3%, EQUIPAR aplica3%. Após force-stop e reabrir o app, selecionar Patrulheiro mostrou inventário12/24 e crítico3%, confirmando persistência em reinício físico. Não houve exceção/FATAL/ANR nos trechos de log do processo examinados.

PSS pontual observado315–329MiB; isso é uma amostra de memória, não uma medição de FPS, CPU, GPU ou vazamentos. Capturas em work/android-qa no workspace Codex. Ainda não foram percorridos todos os heróis/chefes/salas em teste humano prolongado nem medidos todos os critérios de desempenho. Textos de dois HUDs se sobrepõem na arena; limitação visual registrada para a etapa de interface. Não declarar que todos os recursos estão aprovados por esse teste parcial.

Próximo recorte: compilar o cliente cooperativo Android e conectar ao host Windows na mesma rede. Até aqui nenhum teste PC–Android multiplayer foi concluído. A implementação da rede continua na etapa7.
