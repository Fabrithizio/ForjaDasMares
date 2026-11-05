# Kit original — cidade e vegetação

Produção de assets em 05/11/2026, separada do projeto Unity. Não muda a cidade, cenas, scripts, salvamentos ou builds. Modelos originais feitos por script no Blender 4.5.14; não são imagens fingindo ser modelos.

## Onde olhar

- `Previews`: renderizações reais de cada modelo.
- `Modelos`: FBX LOD0 e LOD1, mais a paleta PNG compartilhada.
- `Kit_original_cidade_e_vegetacao.blend`: modelos editáveis, organizados como galeria. Não é o mapa do jogo.
- `Documentacao/CATALOGO.csv`: dimensões em metros, triângulos reais de cada LOD e função.
- `Ferramentas/gerar_kit.py`: fonte reproduzível dos modelos. Reexecutá-lo substitui os arquivos gerados deste kit; para preservar alterações artísticas manuais, salve-as com outro nome antes.
- `Ferramentas/reconstruir_kit.ps1`: fluxo completo de geração, limpeza de malhas, reimportação dos FBX, renderização final e pranchas. Prefira este fluxo à execução isolada do gerador.

## Direção adotada

Fantasia estilizada, madeira quente, pedra, telhas azul-esverdeadas, vegetação com folhas modeladas e pequenos elementos de tecnologia antiga. Usei as referências de cenário e a árvore de copa aberta como orientação, sem tentar reconstruir suas texturas detalhadas. O acabamento é estilizado com paleta de cores; não é pintura manual ou material PBR fotográfico.

Casas com telhas separadas, testeiras, estrutura de madeira, alvenaria, janelas, portas, degraus e variantes. Arbustos com ramificações e folhas; árvores com tronco irregular, raízes, galhos e copa. Props urbanos identificáveis. São primeiras propostas visuais para avaliação, não arte final aprovada.

## Conteúdo

36 assets: duas casas; parede, porta e janela; dois arbustos; duas árvores; samambaia, grama, flores, cogumelos; banco, cerca, poste com lanterna, poço, fonte, placa, floreira, console e marco antigos, carrinho e banca; lenha, cesto, altar, toco, ponte, escada, treliça com trepadeira, toldo, cristais, estandarte, estante e marco de transporte.

Cada asset possui dois FBX de detalhe diferentes. LOD não significa mais um asset de conteúdo. As duas casas são variantes do mesmo conjunto construtivo. Há modelos semelhantes a poço, arbusto e cogumelos no antigo ExpeditionSet: estas são alternativas visuais novas, sem sobrescrever os antigos.

## Importação posterior na Unity

1. Depois da aprovação visual, copiar somente os FBX e `Paleta_original.png` necessários para uma pasta própria em Assets. Não importar o .blend para evitar dependência do Blender no computador de quem abre o projeto.
2. Conferir Scale Factor 1 e altura pelo catálogo. Unidade de produção: metro; Blender Z para cima, exportação FBX Y para cima.
3. Criar um material URP/Lit compartilhado, apontar Base Map para a paleta PNG e atribuí-lo aos modelos. A exportação inclui a imagem, mas a conversão automática de material Blender para URP não foi validada. Paleta: filtro Point, sem mipmaps e compressão desligada para não misturar faixas de cores.
4. Montar prefab com LODGroup, LOD0 e LOD1. Faixas iniciais sugeridas: 0,25 / 0,08 / culling; ajustar pela distância real da câmera. Os dois arquivos são alternativas, não devem ficar renderizando simultaneamente. O LOD1 das plantas reduz folhas inteiras; verificar transição antes de integrar.
5. Colisores simples separados da malha visual: tronco nas árvores; volume do prédio nas casas; base/postes em props quando necessário. Folhas, toldos e flores não precisam bloquear o personagem. Não adicionar MeshCollider em toda a vegetação. Portas e passagens exigem volumes definidos conforme o mapa futuro.
6. Materiais e UV da paleta servem para cor, não têm unwrap adequado para texturas detalhadas, lightmap UV, normal map ou pintura por superfície. Gerar UV2 na importação se usar iluminação baked.
7. Conferir FPS, sombras, colisões e seleção na cena real antes de substituir qualquer placeholder. Android permanece pendente; nenhum APK foi gerado nesta etapa.

## Limites explícitos

Casas são exteriores fechados, sem interiores jogáveis. A porta isolada está em arquivo próprio, mas as portas decorativas das casas inteiras não se abrem automaticamente. Para uma casa acessível, usar parede/porta modular ou editar o edifício. Nenhum modelo deste kit já possui scripts de interação, animações, navegação ou colisores Unity configurados.

Água, cristais e lanternas são geometria e cor. Não incluem iluminação de runtime, partículas, fogo, água simulada, som ou teleporte. Tecido não possui física; vegetação não possui vento. A regra de feedback visual das missões continua obrigatória na integração futura; não está implementada só porque existe o objeto.

Modelos de NPCs, inimigos rigados, animações do protagonista e todo o catálogo de 122 referências não foram produzidos neste kit. O escopo desta entrega é cenário e decoração. O mapa deve ser discutido e construído separadamente.

## Assets existentes preservados

Inspeção de arquivos encontrou personagens Prisma/Brasa, servo e mago de cristal; modelos Meshy de arcos, muro, tenda, braseiros, rochas, árvore antiga, pontes, portões, estátua e urna. O projeto também contém Arsenal e ExpeditionSet. Não apaguei nem renomeei esses arquivos e não os substituí automaticamente.

## Próxima discussão do mapa

Definir primeiro a função e silhueta de cada lugar: chegada segura, praça, oficina, mercado, jardim e saída para exploração. Depois definir circulação, distâncias, áreas de câmera e pontos de interesse. Só então distribuir assets e acertar colisões, iluminação e densidade. Não preencher o mapa aleatoriamente só para ocupar espaço.
