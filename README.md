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

Response Body (JSON):
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

- **O retorno vai variar conforme o prompt e também o schema desejado**

⚙️ Tecnologias Utilizadas
.NET 8 – Backend e APIs

C# – Linguagem principal
Azure OpenAI – Extração de entidades via IA
REST API – Comunicação entre sistemas
Application Insights (opcional) – Observabilidade

🛠️ Como Clonar e Executar
1. Clonar o repositório

git clone https://github.com/seu-repositorio/azure-openai-poc.git
cd azure-openai-poc
2. Configurar variáveis de ambiente
No arquivo appsettings.json ou variáveis do ambiente, configure:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://SEU-ENDPOINT.openai.azure.com/",
    "ApiKey": "SUA-CHAVE-API",
    "Deployment": "gpt-4o-mini"
  }
}

3. Restaurar pacotes
dotnet restore

4. Compilar e rodar
dotnet run

A aplicação iniciará por padrão na porta 5000.

5. Testar endpoint
Utilize curl, Postman ou Insomnia para testar:

curl -X POST http://localhost:5000/extractBatchAzure \
  -H "Content-Type: application/json" \
  -d '{"items": ["Dipirona 500mg comprimido", "Luvas M"]}'

📌 Observações
O parâmetro temperature está configurado para controlar a "criatividade" do modelo:

Valores baixos (ex: 0.0 – 0.3): respostas mais determinísticas.

Valores altos (ex: 0.7 – 1.0): respostas mais criativas.

As respostas são sempre retornadas em JSON estruturado, de acordo com o schema definido.

Em caso de listas grandes, é possível usar batch requests para processar mais de um item por vez.

É recomendado monitorar o uso de tokens para evitar estouro de limite em prompts muito grandes.

📜 Licença
Este projeto foi desenvolvido para fins de POC (Prova de Conceito).
A utilização em produção deve considerar políticas de segurança, performance e custos do Azure.