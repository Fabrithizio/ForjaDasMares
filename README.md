# Forja das Marés — arena jogável

Protótipo original de ação 3D na Unity. Alvos atuais: Windows e Android. Sem multiplayer, inventário ou progressão nesta entrega.

## Abrir o projeto

Instalar Unity 6.3 LTS 6000.3.26f1 pelo Unity Hub, com Android Build Support para testes Android. Adicionar a pasta clonada como projeto e aguardar a resolução dos pacotes.

Abrir `Assets/ForjaDasMares/Scenes/ArenaTest.unity`, ou usar **Forja das Mares → Open playable arena (F6)**. Clicar Play e dar foco à janela Game. Uma cena vazia Untitled não contém o jogo.

## Controles

WASD/setas: mover. Mouse: mirar. Clique esquerdo: atacar. Espaço: esquivar. R: reiniciar. Android usa joystick e botões de toque, em paisagem.

## Validação

Versão Windows compilada e executada no PC. Funcionamento de movimento, ataque e reinício confirmado pelo usuário. 11 testes de PlayMode aprovados após as correções. Android depende do registro de build/teste em `Docs/PROGRESSO.md`.

Os personagens, espada e obstáculos são placeholders geométricos. Não há arte ou animação final nesta etapa.

## Documentação e recuperação

- [Progresso e limitações](Docs/PROGRESSO.md)
- [Guia da arena](Docs/ARENA_COMO_JOGAR.md)
- [Auditoria](Docs/AUDITORIA.md)
- [Arquitetura](Docs/ARQUITETURA.md)

O backup inclui Assets e seus arquivos meta, Packages, ProjectSettings, scripts e documentação. Library/Temp/Logs/Builds são gerados localmente e ficam fora do Git. Para recuperar em outro PC, clonar/baixar o repositório e abrir com a versão indicada acima. Não copie somente os scripts: a cena, materiais, referências e configurações também são necessários.

VS Code com extensões Unity e C# é o editor de código escolhido. Visual Studio IDE não é necessário para esta arena.
