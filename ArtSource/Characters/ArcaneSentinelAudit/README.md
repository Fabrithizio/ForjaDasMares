# Crystal Arcane Sentinel — auditoria de viabilidade

29/10/2026. Inspeção do FBX fornecido, preservando todos os originais. Nenhuma integração, rig, animação ou alteração do jogo foi realizada nesta auditoria.

## Resultado

Bom candidato a reaproveitamento como mago/invocador inimigo. O arquivo contém um objeto de malha com 6.931 vértices e 13.462 triângulos, UV/texturas; não contém Armature, grupos de vértices, pesos, modificadores de esqueleto ou animações. Portanto, este FBX é estático; não é um rig existente desalinhado. Outro arquivo exportado do Meshy pode conter rig, mas ele não estava nesta pasta.

As imagens front.png e side.png são renders de inspeção do modelo real, não conceitos gerados ou captura do jogo. Braços afastados, mãos vazias e pernas visíveis favorecem a criação de um rig. A vista lateral confirma volume; não é apenas uma figura plana. O modelo mede aproximadamente 0,95 unidade de altura após importação FBX; ajustar escala no pipeline sem presumir que a unidade original corresponda à escala do jogo.

Foram contadas 296 arestas de borda e nenhuma aresta compartilhada por mais de duas faces. Há pelo menos vinte componentes conectados, com partes de armadura/roupa separadas. Esses números NÃO comprovam 296 buracos visíveis ou corrupção: podem representar superfícies abertas, separações e fragmentos intencionais. Não soldar tudo automaticamente nem fechar a roupa sem avaliação visual.

## Caminho de reparo recomendado

1. Fazer cópia de trabalho, alinhar frente e escala, conferir normais, superfícies finas e mãos.
2. Criar esqueleto humanoide com cabeça/coluna/braços/mãos/pernas/pés. Máscara e cristais da cabeça devem acompanhar a cabeça; ombreiras precisam de pesos que evitem dobrar como tecido.
3. Vincular malha ao rig e revisar pesos de mangas, ombros, quadril e joelhos. Manto e abas requerem pesos próprios ou alguns ossos auxiliares; não usar simulação de tecido cara como solução inicial.
4. Validar poses exageradas e caminhada. Só então criar/retargetar Idle, Walk/Run, conjuração, golpe recebido e morte. Um rig criado ainda não significa animação aprovada.
5. Criar LODs e versões menores de textura após comparar silhueta/deformação. A contagem atual não exige descartar o modelo nem justifica aceitar redução automática agressiva sem conferir.
6. Integrar ao sistema existente de inimigos; o papel de mago/invocador pode aproveitar componentes atuais e combinar com a criatura quadrúpede já integrada. Essa integração ainda NÃO foi feita.

Não há garantia de peso automático perfeito; viabilidade confirmada por estrutura e aparência, qualidade de deformação ainda precisa ser testada. Não é necessário gastar nova geração no Meshy antes dessa tentativa.

## Evidência e reprodução

inspect.py importa o FBX sem alterar a origem, conta topologia/rig/pesos, salva report.json e renderiza frente/lateral no Blender 4.5.14. Materiais da prévia usam a textura de cor; não é validação final do material PBR na Unity. Primeira tentativa de render detectou World ausente na cena vazia; o script foi corrigido para criar o World e completou as duas imagens. Logs locais: Logs/sentinel-audit.log.

Não foi necessária compilação da Unity: nenhum script de runtime/cena/prefab foi alterado. Nenhum APK ou build novo foi gerado. A versão do jogo permanece 0.23.0.
