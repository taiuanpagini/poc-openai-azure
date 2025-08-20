# POC - Extração de Entidades com Azure OpenAI

Este projeto tem como objetivo validar o uso do **Azure OpenAI** para análise e extração de entidades relacionadas a medicamentos e materiais a partir de textos ou listas de itens.  
A solução foi implementada em **C# (.NET 8)** e exposta como API para facilitar a integração com outros sistemas.

---

## 📌 Motivação e Escolha do Azure OpenAI

Durante a POC (Prova de Conceito), optamos pelo **Azure OpenAI** pelos seguintes motivos:

- **Segurança e conformidade**: hospedagem na infraestrutura Azure, garantindo compliance corporativo (ISO, SOC, GDPR, LGPD).  
- **Integração nativa com Azure**: fácil autenticação via **Managed Identity**, integração com **Application Insights** e outros recursos do Azure.  
- **Escalabilidade**: suporta alto volume de requisições com balanceamento automático.  
- **Gestão centralizada**: controle de custos, políticas de acesso e monitoramento unificados no portal Azure.  
- **Recursos avançados**: uso de schemas JSON para garantir respostas estruturadas, parametrização de temperatura para controle de criatividade e batch requests para processar listas.  

Essa combinação permitiu validar rapidamente a solução, garantindo segurança e flexibilidade para expandir para produção.

---

## 🚀 Endpoints Disponíveis

### `POST /extractBatchAzure`
Extrai entidades de uma lista de itens em lote utilizando Azure OpenAI.  

- **Request Body (JSON):**
```json
{
  "items": [
    "Dipirona 500mg comprimido",
    "Luvas de procedimento tamanho M",
    "Seringa descartável 10ml"
  ]
}
```

- **Response Body (JSON):**
```json
{
  "results": [
    {
      "nome": "Dipirona",
      "categoria": "Medicamento",
      "forma": "Comprimido",
      "dosagem": "500mg"
    },
    {
      "nome": "Luvas de procedimento",
      "categoria": "Material",
      "tamanho": "M"
    },
    {
      "nome": "Seringa descartável",
      "categoria": "Material",
      "capacidade": "10ml"
    }
  ],
  "tokens": {
    "input": 120,
    "output": 95,
    "total": 215
  }
}
```

- **O retorno vai variar conforme o prompt e também o schema desejado**

## ⚙️ Tecnologias Utilizadas
- .NET 8 – Backend e APIs
- C# – Linguagem principal
- Azure OpenAI – Extração de entidades via IA
- REST API – Comunicação entre sistemas
- Application Insights (opcional) – Observabilidade

## ☁️ Como Criar o Projeto no Azure OpenAI

Antes de rodar a POC, você precisa criar o recurso Azure OpenAI no portal do Azure. Siga estes passos:

1. **Acessar o Portal do Azure**
   - Entre no [Portal do Azure](https://portal.azure.com/).

2. **Criar um recurso Azure OpenAI**
   - Clique em **Criar um recurso** > **IA + Machine Learning** > **Azure OpenAI**.
   - Escolha:
     - **Assinatura**: sua assinatura do Azure.
     - **Grupo de Recursos**: ou crie um novo.
     - **Nome**: defina um nome único para o recurso.
     - **Região**: escolha uma região suportada pelo Azure OpenAI.

3. **Definir o nível de preço**
   - Selecione o plano compatível com o volume esperado de requisições (ex.: S0 para teste/POC).

4. **Criar e acessar o recurso**
   - Clique em **Revisar + Criar** e depois em **Criar**.
   - Após a criação, vá até o recurso e copie:
     - **Endpoint**: URL base do serviço.
     - **Chave de API**: necessária para autenticação via `ApiKey`.

5. **Criar o Deployment do modelo**
   - Dentro do recurso, clique em **Explore Azure AI Foundry Portal**.
   - Você será redirecionado para o portal do Foundry, onde poderá criar um deployment:
     - **Nome do deployment**: por exemplo `gpt-4o-mini`.
     - **Modelo**: escolha o modelo que deseja usar (ex.: GPT-4o ou GPT-4o-mini, na POC foi utilizado o modelo GPT-4o-mini pois o custo é bem menor).
     - Clique em **Criar**.
   - Esse **deployment name** será usado na sua aplicação para instanciar o cliente OpenAI.

6. **Testar no portal**
   - Use a aba **Test** no Foundry para enviar prompts e verificar o retorno do modelo antes de integrar na sua API.

> 💡 Dica: você pode usar **Managed Identity** em vez de chave de API para autenticação mais segura, especialmente em produção.

## 🛠️ Como Clonar e Executar
1. Clonar o repositório

- git clone https://github.com/seu-repositorio/azure-openai-poc.git
- cd azure-openai-poc

2. Configurar variáveis de ambiente
- No arquivo appsettings.json ou variáveis do ambiente, configure:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://SEU-ENDPOINT.openai.azure.com/",
    "ApiKey": "SUA-CHAVE-API",
    "Deployment": "DEPLOY-CRIADO"
  }
}
```

3. Restaurar pacotes
- dotnet restore

4. Compilar e rodar
- dotnet run

**A aplicação iniciará por padrão na porta 5000.**

5. Testar endpoint
- Utilize curl, Postman ou Insomnia para testar:

```bash
curl -X POST http://localhost:5000/extractBatchAzure \
  -H "Content-Type: application/json" \
  -d '{"items": ["Dipirona 500mg comprimido", "Luvas M"]}'
```

## 📌 Observações
- O parâmetro temperature está configurado para controlar a "criatividade" do modelo:

- Valores baixos (ex: 0.0 – 0.3): respostas mais determinísticas.

- Valores altos (ex: 0.7 – 1.0): respostas mais criativas.

- As respostas são sempre retornadas em JSON estruturado, de acordo com o schema definido.

- Em caso de listas grandes, é possível usar batch requests para processar mais de um item por vez.

- É recomendado monitorar o uso de tokens para evitar estouro de limite em prompts muito grandes.

## 📜 Licença
- Este projeto foi desenvolvido para fins de POC (Prova de Conceito).
- A utilização em produção deve considerar políticas de segurança, performance e custos do Azure.