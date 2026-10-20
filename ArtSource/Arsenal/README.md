# Arsenal original — versão 0.13

Fonte criada para Forja das Marés no Blender 4.5.14. Não utiliza modelos, texturas, sons ou animações de terceiros. São modelos estilizados iniciais, não arte final. O Brasa original foi preservado em ../Characters/Brasa/Brasa.blend; este trabalho foi salvo separadamente em Brasa_Arsenal.blend.

create_arsenal.py carrega o rig original, acrescenta doze ações com keyframes reais nos ossos e exporta o FBX Brasa existente, mantendo os treze clipes anteriores. Cria também nove modelos estáticos, cinco materiais compartilhados e nove ícones renderizados com fundo transparente. Caminhos de entrada/saída estão no início do script e devem ser ajustados em outra máquina. Executar pelo Blender em background com --python create_arsenal.py. A prévia e arsenal_report.json registram o resultado.

Na Unity, Networking/Editor/ArsenalInstaller.Upgrade importa e normaliza os modelos, cria meshes persistentes, configura os ícones, acrescenta estados ao Animator e atribui as referências aos dados existentes. Não executar os antigos instaladores Forge/Rpg para atualizar esta revisão: eles recriam valores anteriores. Faça backup antes de regenerar qualquer conteúdo.

Espada, machado e martelo possuem conjuntos de ataques próprios no Brasa. Os outros heróis conservam suas animações anteriores; ter um modelo de arco ou cajado não significa ter um novo conjunto de animações para essas classes. Meshes: 472–764 triângulos cada, conforme relatório. Brasa usa root motion desligado e mantém a caminhada vinculada à distância percorrida. Substituir os clipes no controller preservando nomes, ActionTime e a máscara da camada superior.
