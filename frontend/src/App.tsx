import { useEffect, useState } from "react";
import "./App.css";

type StatusApi = {
  mensagem: string;
};

function App() {
  const [mensagem, setMensagem] = useState("Conectando à API...");

  useEffect(() => {
    async function consultarApi() {
      try {
        const resposta = await fetch(`${import.meta.env.VITE_API_URL}/`);

        if (!resposta.ok) {
          throw new Error("A API respondeu com erro.");
        }

        const dados: StatusApi = await resposta.json();
        setMensagem(dados.mensagem);
      } catch {
        setMensagem("Não foi possível conectar à API.");
      }
    }

    consultarApi();
  }, []);

  return (
    <main>
      <h1>JotaNunesForms</h1>
      <p>{mensagem}</p>
    </main>
  );
}

export default App;