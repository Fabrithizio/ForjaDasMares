# Cidade Viva — revisão ativa de superfícies

Esta é a revisão usada pelo CityLifeInstaller a partir de 0.30.0. Não é um pacote de novos sistemas.

- Cidade_viva_refinada.blend: fonte editável atual, com imagens de superfície empacotadas.
- Modelos: 18 modelos, 36 FBX LOD0/LOD1, atlas compartilhado 768 x 512.
- Previews: renderizações reais dos modelos refinados.
- Documentacao/CATALOGO.csv: dimensões e triângulos; validacao_fbx.json: reimportação dos FBX.
- ../Ferramentas/refinar_cidade_viva.py: refaz a revisão a partir da fonte base, sem sobrescrevê-la.

Mudanças: UVs de superfície por face, textura procedural de madeira/pedra/tecido/metal no mesmo atlas, normais suavizadas por ângulo sem subdividir as malhas. São superfícies procedurais estilizadas, não pintura manual nem texturas PBR finais. Placas de padaria, ervas e oficina têm símbolos modelados e não implementam lojas.

Para reproduzir: primeiro garantir a fonte base Cidade_viva_editavel.blend; rodar Blender --background --factory-startup --disable-autoexec --threads 2 --python-exit-code 1 --python ArtSource/CityLifeKit/Ferramentas/refinar_cidade_viva.py; depois validar_refinado.py. Preservar alterações manuais em cópia antes de regenerar.

Na Unity são mantidos nomes/caminhos dos modelos e seus GUIDs; o material passa a usar filtragem bilinear, mipmaps, compressão e gutters no atlas. Conferir as transições e mistura de cores em dispositivo físico; não há garantia universal de qualidade visual por esta configuração.
