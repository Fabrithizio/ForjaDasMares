# Forja das Marés — arena jogável

Protótipo original de ação 3D na Unity. Alvos atuais: Windows e Android. Sem multiplayer, inventário ou progressão nesta entrega.

## Abrir o projeto

Instalar Unity 6.3 LTS 6000.3.26f1 pelo Unity Hub, com Android Build Support para testes Android. Adicionar a pasta clonada como projeto e aguardar a resolução dos pacotes.

Abrir `Assets/ForjaDasMares/Scenes/ArenaTest.unity`, ou usar **Forja das Mares → Open playable arena (F6)**. Clicar Play e dar foco à janela Game. Uma cena vazia Untitled não contém o jogo.

## Controles

WASD/setas: mover. Mouse: mirar. Clique esquerdo: atacar. E/clique direito: habilidade. Espaço: esquivar. 1–4/Tab: trocar herói. R: reiniciar. Android usa joystick fixo e botões de ataque/esquiva/habilidade, em paisagem.

## Validação

Base Windows 0.1.0 confirmada pelo usuário. Versão 0.2.0 recompilada, executada e conferida pelos testes nesta sessão. Etapa 3: quatro heróis modulares e 23 testes de PlayMode aprovados após as correções. Joystick segurado e efeitos de cada herói verificados no moto g15. APK ARM64 gerado, instalado e testado no moto g15 com Android 15: movimento, ataque, esquiva e reinício conferidos. A invocação interna do Gradle pela Unity nesta sessão apresentou uma limitação Java; a montagem direta pelo Gradle foi concluída. Detalhes e procedimento em Docs/PROGRESSO.md.

Os personagens, espada e obstáculos são placeholders geométricos. Não há arte ou animação final nesta etapa.

## Documentação e recuperação

- [Progresso e limitações](Docs/PROGRESSO.md)
- [Guia de heróis e combate](Docs/HEROIS_COMO_TESTAR.md)
- [Guia da arena](Docs/ARENA_COMO_JOGAR.md)
- [Auditoria](Docs/AUDITORIA.md)
- [Arquitetura](Docs/ARQUITETURA.md)

O backup inclui Assets e seus arquivos meta, Packages, ProjectSettings, scripts e documentação. Library/Temp/Logs/Builds são gerados localmente e ficam fora do Git. Para recuperar em outro PC, clonar/baixar o repositório e abrir com a versão indicada acima. Não copie somente os scripts: a cena, materiais, referências e configurações também são necessários.

VS Code com extensões Unity e C# é o editor de código escolhido. Visual Studio IDE não é necessário para esta arena.

