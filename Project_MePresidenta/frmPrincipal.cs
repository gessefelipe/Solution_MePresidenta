using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using MePresidentaServidor;

namespace Project_MePresidenta
{
    public partial class frmPrincipal : Form
    {
        enum STATUS { PARTIDAABERTA, PARTIDAINICIADA, PARTIDAENCERRADA, MINHAVEZ, POSICIONAR, PROMOVER, VOTAR }
        enum ALGORITMO
        {
            posicionamento0Padrao, promocao1Padrao, votacao2Padrao
                        , posicionamento3Equilibrado, promocao4MeudeMenorNivel, votacao5TenhoPontosSuficientes
                        , promocao7QualquerdeMenorNivel,promocao10IniciarMeuReinado, promocao13OutrosdeMaiorNivel

        }

        public int[] _algoritmoPromocaoPrioridade =
            {0,(int)ALGORITMO.promocao10IniciarMeuReinado
            ,(int)ALGORITMO.promocao4MeudeMenorNivel
            ,(int)ALGORITMO.promocao13OutrosdeMaiorNivel
            ,(int)ALGORITMO.promocao7QualquerdeMenorNivel
            ,(int)ALGORITMO.promocao1Padrao
        };

        public int _algoritmoPromocao;
        public int _algoritmoPosicionamento;
        public int _algoritmoVotacao;

        public int _algoritmoPromocaoPrioridadeAtual;

        //PROPRIEDADES
        public int _myId;
        public string _mySenha;
        public int _partidaId;
        public string _partidaSenha;
        //FLAGS DE CONTROLE
        public int _partidaStatus;
        public bool _rotinaPartidaIniciada;
        public bool _rotinaPartidaEncerrada;
        public bool _autojogo;
        public bool _TimerHabilitado;
        public bool _ConfiguracoesHabilitado;
        public int _nrodadaAtual;
        public bool _primeirajogadarealizada;
        public int _candidato;
        public bool _mostrarPaineldeInformacoes;
        public bool _ativarDebug;
        public bool _forceNovaRodada;
        //PONTOS
        public int _pontuacaoAtual;
        public int _pontuacaoTotal;
        //JOGADORES
        public int _nJogadores;
        public int _nCartasNaoMax;
        public int _meuCodigoJogador;
        public int[] _jogadoresId = new int[6];
        public string[] _jogadoresNome = new string[6];
        public int[] _jogadoresPontos = new int[6];
        public int[] _jogadoresCartasNaoDiponiveis = new int[6];//numero maximo de jogadores = 6
        public string[] _jogadoresHistoricoVotos = new string[6];
        public string[] _jogadoresUltimaJogadaAnalisada = new string[6];
        public int[] _jogadoresJogadacontador = new int[6];
        public int _jogadorQueCandidatou;

        //PERSONAGENS
        public int _nPersonagens;
        public int[] _personagemStatus = new int[13];
        //-2 eliminado
        //-1 nao posicionado
        //x posicionado no novel x
        public bool[] _personagemMeu = new bool[13];
        public int _personagemEliminar;
        public string[] _personagemCodinome = new string[13];
        public string[] _personagemNome = new string[13];

        //NIVEIS
        public int[] _nivelQtdPersonagens = new int[11];


        //VOTACAO

        //ARQUIVO DEBUG
        public StreamWriter debugfile;
        public string debugfilepath;
        public int debugfilecode;
        public int _counterDebug;
        public string[] _debuginfileLastRetornoServidor = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
        public string _debuginfileLastVariaveisGlobais;

        //ACEBILIDADE
        public int _PersonagemSelecionado;


        //ALGORITMOS
        
        public int[,] _jogadoresCartas = new int[,] { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        public int[,] _contadorPromocaoJogadorPersonagem = new int[,] { { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };


        public int _calculoMinhaPontuacaoAtual;
        public int _calculoMinhaPontuacaoTotalAtual;
        public int _calculoMaiorPontuacaoPossivelAtual;
        public int _calculoMaiorPontuacaoTotalAtualPossivel;
        public bool _calculoMinhaPontuacaoeBoa;
        public int _calculoQtdCartasNaoQueNaoSaoMinhas;
        public int _calculoQtdMediaCartasNao;
        public int _calculoQtdTotalAtualCartasNaoNoJogo;


        public frmPrincipal()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);


            _counterDebug = 0;//utilizado no debug
            debugfilepath = GetTimestamp(DateTime.Now);
            debugfilecode = 0;
            _debuginfileLastVariaveisGlobais = "";
            _ativarDebug = true;
            
            mostrarOcultarDebug();

            lblVersao.Text = "MePresidentaServidor.dll | Versão " + Jogo.versao;
            lblDebugFileName.Text += debugfilepath;

            lblArquivoDebug.Text = "Arquivo Debug: " + lblDebugFileName.Text + ".txt";

            
            _ConfiguracoesHabilitado = false;
            botaoNaoDestacarconfiguracao(btnConfiguracoes);
            _TimerHabilitado = true;
            botaoDestacarConfiguracaoHabilitada(btnHabilitarTimer);

            inicio();
        }

        
        public void inicio()
        {

            lblIdPartida.Text = "";
            labelNomePartidaSelecionada.Text = "";

            listarPartidas();
            inicializarVariaveisGlobais();
            inicializarJogadoresCartas();

            _mostrarPaineldeInformacoes = true;
            mostarOcultarInformacoes();
            _forceNovaRodada = false;

            _PersonagemSelecionado = -1;

            //padronizar criacao partida para testes
            txtNomedaPartida.Text = "Londres";
            txtSenhaPartida.Text = "123";
            paineis(-1);
            debugVariaveisGlobais("FormLoad");
            jogadoresLimparPainel();
            timer1.Enabled = true;

            lblNumRodada.Text = "0/3";
            lblPontuacaoAtual.Text = "0";
            lblPontuacaoTotal.Text = "0";

        }

        public void inicializarCalculos()
        {
            _calculoMinhaPontuacaoAtual = 0;
            _calculoMinhaPontuacaoTotalAtual = 0;
            _calculoMaiorPontuacaoPossivelAtual = 0;
            _calculoMaiorPontuacaoTotalAtualPossivel = 0;
            _calculoMinhaPontuacaoeBoa = false;
            _calculoQtdCartasNaoQueNaoSaoMinhas = 0;
            _calculoQtdMediaCartasNao = 0;
            _calculoQtdTotalAtualCartasNaoNoJogo = 0;
        }


        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        


        public void novaRodada()
        {
            debug("novaRodada()",true,true);
            _nPersonagens = listarPersonagens();
            inicializarNiveis();
            inicializarCartasNao();
            inicializarJogadoresCartas();
            inicializarCalculos();
            jogadoresLimparVotacao(-1);
            listarCartas();
            tabuleiroLimpar();
            gbVotacao.Visible = false;
            _personagemEliminar = -1;
        }

        public void paineis(int status)
        {
            switch (status)
            {
                case -1:
                    painelTabuleiro.Visible = false;
                    painelLobby.Visible = true;
                    btnIniciarPartida.Enabled = false;
                    break;
                case (int)STATUS.PARTIDAABERTA:
                    painelLobby.Visible = false;
                    if (_nJogadores > 1)
                    {
                        btnIniciarPartida.Enabled = true;
                    }
                    else
                    {
                        btnIniciarPartida.Enabled = false;
                    }
                    painelTabuleiro.Visible = true;
                    //btnAutoJogo.Text = "Configurações";
                    /*gbVotacao.Visible = false;
                    gbPosicionamento.Visible = false;
                    gbMinhasCartas.Visible = false;*/
                    break;
                case (int)STATUS.PARTIDAINICIADA:
                    painelLobby.Visible = false;
                    btnIniciarPartida.Enabled = false;
                    painelTabuleiro.Visible = true;
                    //btnAutoJogo.Text = "Ativar Jogadas Automáticas"; 
                    break;
                case (int)STATUS.PARTIDAENCERRADA:
                    painelLobby.Visible = false;
                    btnIniciarPartida.Enabled = false;
                    painelTabuleiro.Visible = true;
                    break;
            }
        }




        public string servidor(int metodo)
        {
            int tentativa = 0;
            debug("servidor("+metodo+")");
            string texto = null;
            string metodoName = "";
            while (true)
            {
                try
                {
                    switch (metodo)
                    {
                        case 0:
                            texto = Jogo.ListarPartidas();
                            metodoName = "ListarPartidas";
                            break;
                        case 1:
                            texto = Jogo.VerificarVez(_myId);
                            metodoName = "VerificarVez";
                            break;
                        case 2:
                            texto = Jogo.ListarJogadores(_partidaId);
                            metodoName = "ListarJogadores";
                            break;
                        case 3:
                            texto = Jogo.ListarCartas(_myId, _mySenha);
                            metodoName = "ListarCartas";
                            break;
                        case 4:
                            texto = Jogo.ListarPersonagens();
                            metodoName = "ListarPersonagens";
                            break;
                        case 5:
                            texto = Jogo.ListarSetores();
                            metodoName = "ListarSetores";
                            break;
                        case 6:
                            //if (!validarCriacaoPartida()) break;
                            texto = Jogo.CriarPartida(txtNomedaPartida.Text, txtSenhaPartida.Text);
                            metodoName = "CriarPartida";
                            break;
                        case 7:
                            texto = Jogo.Entrar(Convert.ToInt32(lblIdPartida.Text), txtNomedoJogador.Text, txtSenhadaPartidaEntrar.Text);
                            metodoName = "Entrar";
                            break;
                        case 8:
                            texto = Jogo.ColocarPersonagem(_myId, _mySenha, Convert.ToInt32(txtSetor.Text), txtPersonagem.Text);
                            metodoName = "ColocarPersonagem";
                            break;
                        case 9:
                            texto = Jogo.Iniciar(_partidaId, _partidaSenha);
                            metodoName = "Iniciar";
                            break;
                        case 10:
                            texto = Jogo.Promover(_myId, _mySenha, txtPersonagem.Text);
                            metodoName = "Promover";
                            break;
                        case 11:
                            texto = Jogo.Votar(_myId, _mySenha, "S");
                            metodoName = "Votar";
                            break;
                        case 12:
                            texto = Jogo.Votar(_myId, _mySenha, "N");
                            metodoName = "Votar";
                            break;
                        case 13:
                            texto = Jogo.ExibirUltimaVotacao(_myId, _mySenha);
                            metodoName = "ExibirUltimaVotacao";
                            break;
                        case 14:
                            texto = Jogo.VerificarStatus(_myId);
                            metodoName = "VerificarStatus";
                            break;
                        case 15:
                            texto = Jogo.ConsultarHistorico(_myId, _mySenha, false);
                            metodoName = "ConsultarHistorico";
                            break;
                    }
                    if (texto.Equals(_debuginfileLastRetornoServidor[metodo]))
                    {
                        debuginfile("RETORNO SERVIDOR " + metodoName + ": No Modifications", "RetornoServidor");
                    }
                    else
                    {
                        debuginfile("RETORNO SERVIDOR " + metodoName + ": " + texto, "RetornoServidor");
                        _debuginfileLastRetornoServidor[metodo] = texto;
                    }
                    if (!validarResultadoServidor(texto, metodoName))
                    {
                        debug("servidor(" + metodoName + ")=RETORNO INVALIDO");
                        return null;
                    }

                    if (texto == "")
                    {
                        debug("servidor(" + metodoName + ")=RETORNOU VAZIO");
                        return null;
                    }

                    debug("servidor(" + metodoName + ")=OK");
                    return texto;
                }
                catch (Exception e)
                {
                    debug("servidor(" + metodo + ")=ERRO DE SERVIDOR - "+e.Message);
                    MessageBox.Show("ERRO DE SERVIDOR(Tentativa "+(++tentativa)+" de 5):" + e.Message);
                    if (tentativa > 5)
                    {
                        return null;
                    }
                    continue;
                }
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!_TimerHabilitado)
            {
                return;
            }
            
            timer1.Enabled = false;

            rotinaPrincipal();

            timer1.Enabled = true;
        }

        private bool validarResultadoServidor(string result, string metodoName)
        {
            
            if (result.Length < 4) return true;
            if (result.Substring(0, 4) == "ERRO")
            {
                debugUser(result+"(" + metodoName + ")", false);
                return false;
            }
            debugUser("Sucesso...(" + metodoName + ")", true);
            return true;
        }

        public void debugUser(string texto,bool sucesso)
        {
            lblErro.Text = texto;
            if (sucesso)
            {
                lblErro.ForeColor = Color.DeepSkyBlue;
                debug(texto);
            }
            else
            {
                lblErro.ForeColor = Color.Red;
                labelIndicadordeErro.Visible = true;
                debug(texto);
            }
        }

        
        public bool verificarVez()
        {
            debug("verificarVez()");

            bool NovaRodada = false;

            string result = servidor(1);
            if (result == null)
            {
                debug("ERROR: verificarVez=false - servidor");
                return false;
            }

            //Analizar jogador da vez
            int jogadordaVez;
            string texto = result.Substring(0, result.Length - 2);
            int posicaoPrimeiraQuebra = result.IndexOf('\r');
            if (!Int32.TryParse(result.Substring(0, posicaoPrimeiraQuebra), out jogadordaVez))
            {
                debug("ERROR: verificar vez= JOGADOR INDEFINIDO",true,true);
                return false;
            }
            debug("jogadordaVez = " + jogadordaVez + " - " + _jogadoresNome[getJogadorIndex(jogadordaVez)]);

            //atualizar tabuleiro
            //forceNovaRodada - forca o reset do tabuleiro para o caso de se iniciar uma nova rodada mas VerificarJogo retornar uma ou mais jogadas subsequentes
            //forceNovaRodada é definida true em analisarUltimaVotacao
            if (_forceNovaRodada || !(texto.Length > posicaoPrimeiraQuebra))//tabuleiro vazio
            {
                if(_forceNovaRodada) debug("verificarVez()=Forcenovarodada");
                else
                {
                    debug("verificarVez()=Tabuleiro vazio");
                }

                if (_primeirajogadarealizada)//só chama novaRodada() se houve ao menos uma jogada. Caso seja a rodada 1, nova rodada foi chamada ao iniciarPartida
                {
                    debug("verificarVez()=Ja houve jogadas - chamar novarodada()");
                    lblNumRodada.Text = Convert.ToString(++_nrodadaAtual + 1) + "/3";
                    _primeirajogadarealizada = false;
                    novaRodada();
                }
                _pontuacaoTotal += _pontuacaoAtual;
                _pontuacaoAtual = 0;
                listarJogadores();//para atualizar pontuacao
                jogadoresMostrar();
                _forceNovaRodada = false;
                NovaRodada = true;
            }


            if (texto.Length > posicaoPrimeiraQuebra)//tabuleiro cheio
            {
                debug("verificarVez()=>tabuleiroAtualizar()");
                _primeirajogadarealizada = true;
                tabuleiroAtualizar(texto.Substring(posicaoPrimeiraQuebra + 2));
                analisarUltimasJogadas();
                _pontuacaoAtual = calculoMinhaPontuacaoAtual();
            }
            


            lblPontuacaoAtual.Text = Convert.ToString(_pontuacaoAtual);
            lblPontuacaoTotal.Text = Convert.ToString(_pontuacaoTotal);


            int candidato = getCandidato();

            if (candidato > -1)//atualizar label de votacao
            {
                debug("verificarVez()= hora de votar");
                labelPerguntaVotacao.Text = "Você escolhe o candidato "+_personagemNome[candidato]+" para ser Presidente ?";
                gbVotacao.Visible = true;
                jogadoresMostrarQueVotou(jogadordaVez);
            }
            else
            {
                debug("nao ha candidatos");
                gbVotacao.Visible = false;
            }



            if ((_candidato > -1) && (candidato < 0))//nao e hora de votar mas houve votacao
            {
                debug("verificarVez()= Houve uma votacao");
                if (!NovaRodada)//analisar ultima votacao apenas se nao é o caso de nova rodada. Caso contrario analisaria duas vezes a mesma votacao
                {
                    analisarUltimaVotacao(_candidato);
                }
                else
                {
                    NovaRodada = false;
                }
                
                _candidato = -1;
            }

            if (_forceNovaRodada)
            {//resolve o problema de outros jogadores terem realizado jogadas na nova rodada mas eu ainda nao iniciei nova rodada

                return verificarVez();

            }

            jogadoresDestacar(jogadordaVez, _meuCodigoJogador);
            tabuleiroPosicionarDesempregados();

            if (_myId == jogadordaVez)
            {
                debug("MINHA VEZ=TRUE",true,true);
                return true;
            }

            debug("MINHA VEZ=FALSE",true,true);
            return false;
        }

        public void mostrarUltimaJogadaJogador(int jogador, string jogada)
        {
            switch (jogador)
            {
                case 0:
                    lblJogadas0.Text = jogada;// + " ";// + lblJogadas0.Text;
                    break;
                case 1:
                    lblJogadas1.Text = jogada;// + " ";// + lblJogadas1.Text;
                    break;
                case 2:
                    lblJogadas2.Text = jogada;// + " ";// + lblJogadas2.Text;
                    break;
                case 3:
                    lblJogadas3.Text = jogada;// + " ";// + lblJogadas3.Text;
                    break;
                case 4:
                    lblJogadas4.Text = jogada;// + " ";// + lblJogadas4.Text;
                    break;
                case 5:
                    lblJogadas5.Text = jogada;// + " ";// + lblJogadas5.Text;
                    break;

            }
        }

        public void algoritmoMostrarDestacar(int algoritmoPosicionamentotemp,int algoritmoPromocaotemp,int algoritmoVotacaotemp)
        {
            int algoritmoPosicionamento = _algoritmoPosicionamento;
            int algoritmoPromocao = _algoritmoPromocao;
            int algoritmoVotacao = _algoritmoVotacao;

            if (algoritmoPosicionamentotemp > 0) algoritmoPosicionamento = algoritmoPosicionamentotemp;
            if (algoritmoPromocaotemp > 0) algoritmoPromocao = algoritmoPromocaotemp;
            if (algoritmoVotacaotemp > 0) algoritmoVotacao = algoritmoVotacaotemp;


            algoritmoNaoDestacar();
            
            switch (algoritmoPosicionamento)
            {
                case (int)ALGORITMO.posicionamento0Padrao:
                    botaoDestacarConfiguracaoHabilitada(btnAlg0);
                    break;
                case (int)ALGORITMO.posicionamento3Equilibrado:
                    botaoDestacarConfiguracaoHabilitada(btnAlg3);
                    break;
            }
            switch (algoritmoPromocao)
            {
                case (int)ALGORITMO.promocao1Padrao:
                    botaoDestacarConfiguracaoHabilitada(btnAlg1);
                    break;
                case (int)ALGORITMO.promocao4MeudeMenorNivel:
                    botaoDestacarConfiguracaoHabilitada(btnAlg4);
                    break;
                case (int)ALGORITMO.promocao7QualquerdeMenorNivel:
                    botaoDestacarConfiguracaoHabilitada(btnAlg7);
                    break;
                case (int)ALGORITMO.promocao10IniciarMeuReinado:
                    botaoDestacarConfiguracaoHabilitada(btnAlg10);
                    break;
                case (int)ALGORITMO.promocao13OutrosdeMaiorNivel:
                    botaoDestacarConfiguracaoHabilitada(btnAlg13);
                    break;
            }
            switch (algoritmoVotacao)
            {
                case (int)ALGORITMO.votacao2Padrao:
                    botaoDestacarConfiguracaoHabilitada(btnAlg2);
                    break;
                case (int)ALGORITMO.votacao5TenhoPontosSuficientes:
                    botaoDestacarConfiguracaoHabilitada(btnAlg5);
                    break;
            }

        }

        
        public void algoritmoNaoDestacar()
        {
            botaoNaoDestacarconfiguracao(btnAlg0);
            botaoNaoDestacarconfiguracao(btnAlg3);

            botaoNaoDestacarconfiguracao(btnAlg1);
            botaoNaoDestacarconfiguracao(btnAlg4);
            botaoNaoDestacarconfiguracao(btnAlg7);
            botaoNaoDestacarconfiguracao(btnAlg10);
            botaoNaoDestacarconfiguracao(btnAlg13);

            botaoNaoDestacarconfiguracao(btnAlg2);
            botaoNaoDestacarconfiguracao(btnAlg5);
        }


        public int analisarStatus()
        {
            string status = servidor(14);
            if (status == null)
            {
                debug("analisarStatus()=Aberta porque status é null");
                return (int)STATUS.PARTIDAABERTA;
            }

            string[] campos = status.Split(',');
            string statusPartida = campos[0];
            string statusRodada = campos[1];

            if (statusPartida.Equals("A"))
            {
                debug("analisarStatus()=Aberta");
                return (int)STATUS.PARTIDAABERTA;
            }
            else if (statusPartida.Equals("J"))
            {
                debug("analisarStatus()=Em jogo");
                if (statusRodada.Equals("S"))
                {
                    debug("analisarStatus()=Posicionar");
                    return (int)STATUS.POSICIONAR;
                }
                else if (statusRodada.Equals("J"))
                {
                    debug("analisarStatus()=Promover");
                    return (int)STATUS.PROMOVER;
                }
                else if (statusRodada.Equals("V"))
                {
                    debug("analisarStatus()=Votar");
                    return (int)STATUS.VOTAR;
                }
            }
            else if (statusPartida.Equals("E"))
            {
                debug("analisarStatus()=Encerrada");
                return (int)STATUS.PARTIDAENCERRADA;

            }
            debug("ERRO: analisarStatus = INDEFINIDO",true,true);

            return -1;
        }
        private void analisarUltimaVotacao(int candidato)
        {
            
            bool houveVotoNao = false;

            string result = servidor(13);
            if (result == null) {
                debug("ERROR: analisarUltimaVotacao()=null",true,true);
                return;
            }


            debug("analisarUltimaVotacao()="+result);

            string texto = result.Substring(0, result.Length - 2);

            string[] votos = texto.Split('\r');

            int i = 0;
            foreach (String voto in votos)
            {
                string voto2 = voto.Replace('\n', ' ');

                string[] campos = voto2.Split(',');

                string id = campos[0].Trim();
                string escolhavoto = campos[1].Trim();

                _jogadoresHistoricoVotos[i] += _personagemCodinome[candidato] + escolhavoto + " ";
                //mostrarUltimaJogadaJogador(i, "("+Convert.ToString(++_jogadoresJogadacontador[i]) + ") " + escolhavoto);//_personagemCodinome[candidato] + 

                jogadoresMostrarVotacao(i, escolhavoto);

                if (escolhavoto.Equals("N"))
                {
                    //se jogador votou nao carta com certeza nao lhe pertence
                    houveVotoNao = true;
                    _jogadoresCartasNaoDiponiveis[i]--;
                    _jogadoresCartas[i, candidato] = 0;//Personagem com certeza não pertece ao jogador
                    if(_personagemEliminar == candidato)
                    {
                        _personagemEliminar = -1;//reset personagem eliminar
                    }
                    debug("analisarUltimaVotacao()=Jogador votou Nao - Personagem nao lhe pertence"+voto2, false, false,true);
                }
                else
                {
                    //se numeros de cartaz nao do jogador é 0, ha poucos nao no jogo e foi ele quem promoveu a carta era dele
                    if ((_calculoQtdTotalAtualCartasNaoNoJogo < _calculoQtdMediaCartasNao) && (_jogadoresCartasNaoDiponiveis[i] == 0) && _jogadoresUltimaJogadaAnalisada[i].Equals("J" + _personagemCodinome[candidato] + "10"))
                    {
                        debug("analisarUltimaVotacao()=Jogador votou Sim, nao tem cartas naos,ha poucos nao no jogo e foi ele quem promoveu o personagem a presidente - Personagem lhe pertence" + voto2, false, false, true);
                        _jogadoresCartas[i, candidato] += 2;
                    }

                    //se há um numero consideravel de cartas nao e o jogadorr possui cartas nao e nao ha muitos personagens eliminados a carta nao é dele*
                    if ((_calculoQtdTotalAtualCartasNaoNoJogo > _calculoQtdMediaCartasNao && calculoNumeroPersonagensEliminados() < 5)&&_jogadoresCartasNaoDiponiveis[i]>0)
                    {

                        debug("analisarUltimaVotacao()=Jogador votou Sim,jogador possui naos ,ha muitos naos no jogo e nao ha muitos personagens eliminados - Personagem Nao lhe pertence" + voto2, false, false, true);
                        _jogadoresCartas[i, candidato] = 0;
                    }



                    
                    if(_jogadoresCartas[i, candidato] > 0)
                    {
                        debug("analisarUltimaVotacao()=Jogador votou Sim, Personagem pode ser dele" + voto2, false, false, true);
                        _jogadoresCartas[i, candidato]++;
                    }
                    
                }
                i++;
            }

            if (houveVotoNao)
            {
                _personagemStatus[candidato] = -2;//eliminado
                _nivelQtdPersonagens[10] = 0;
                eliminarCartaJogadoresCarta(candidato);
                debug("analisarUltimaVotacao()=Personagem eliminado - continuar rodada",true,true);

            }
            else
            {//só houve voto sim - forçar limpeza do tabuleiro
                debug("analisarUltimaVotacao()=Todos votos SIM - Forçar nova rodada",true,true);
                _forceNovaRodada = true;
            }

        }
        

        public void analisarUltimasJogadas()
        {
            string result = servidor(15);

            debug("analisarUltimasJogadas()= " + result);

            if (result == null)
            {
                return;
            }
            


            string texto = result.Substring(0, result.Length - 2);

            string[] jogadas = texto.Split('\r');


            foreach (String jogada in jogadas)
            {
                //ListViewItem item = new ListViewItem();

                string jogada2 = jogada.Replace('\n', ' ');

                string[] campos = jogada2.Split(',');

                string nomejogador = campos[0].Trim();
                string tipojogada = campos[1].Trim();
                string personagem = campos[2].Trim();
                string setor = campos[3].Trim();

                int jogadorindex = getJogadorIndex(nomejogador);
                int personagemindex = getPersonagemIndex(personagem);


                


                //analise
                if (jogadorindex > -1)
                {
                    //if (jogadorindex == _meuCodigoJogador) continue;
                    if (_jogadoresUltimaJogadaAnalisada[jogadorindex].Equals(tipojogada + personagem + setor)) continue;

                    if (setor.Equals("10"))//Mostrar quem candidatou o personagem
                    {
                        jogadoresLimparVotacao(jogadorindex);
                    }


                    _jogadoresJogadacontador[jogadorindex]++;
                    if (tipojogada.Equals("S"))//Posicionamerto
                    {
                        if (setor.Equals("1"))//posicionamento no novel 1 supoe-se que personagem nao e do jogador
                        {
                            debug("analisarUltimasJogadas()=Posicionamento no nivel 1 supoe que personagem nao é do jogador - "+jogada2,false,false,true);
                            _jogadoresCartas[jogadorindex, personagemindex] = 0;
                        }
                    }
                    else if (tipojogada.Equals("J"))//PROMOCAO
                    {
                        _contadorPromocaoJogadorPersonagem[jogadorindex, personagemindex]++;
                        if (_contadorPromocaoJogadorPersonagem[jogadorindex, personagemindex] > 2)//jogador ja promoveu personagem 3 vezes
                        {
                            debug("analisarUltimasJogadas()=Jogador promove muito este personagem - " + jogada2,false,false,true);
                            _jogadoresCartas[jogadorindex, personagemindex]++;
                        }
                        if (setor.Equals("1"))//promocao para o nivel 1 supoe-se que personagem é do jogador
                        {
                            debug("analisarUltimasJogadas()=Promoveu personagem de nivel baixo - " + jogada2,false,false,true);
                            _jogadoresCartas[jogadorindex, personagemindex]++;
                        }

                    }

                    _jogadoresUltimaJogadaAnalisada[jogadorindex] = tipojogada + personagem + setor;
                    mostrarUltimaJogadaJogador(jogadorindex, "("+Convert.ToString(_jogadoresJogadacontador[jogadorindex]) + ") " + personagem + setor);// + tipojogada
                }


            }

            debug("analisarUltimasJogadas() - "+result+"Novo Status JogadoresCartas:", false, false, true);
            debug(debugtextoJogadoresCartas(),false,false,true);

        }

        public void botaoDestacarMeuPersonagem(System.Windows.Forms.Button b)
        {
            b.BackColor = Color.DeepSkyBlue;
            b.ForeColor = Color.FromArgb(9, 18, 23);
        }
        public void botaoDestacarJogadordaVez(System.Windows.Forms.Button b)
        {
            b.BackColor = Color.Black;
            b.ForeColor = Color.White;
        }

        public void botaoDestacarConfiguracaoHabilitada(System.Windows.Forms.Button b)
        {
            b.BackColor = Color.Gold;
            //b.ForeColor = Color.White;
        }

        public void botaoDestacarPersonagemEliminado(System.Windows.Forms.Button b)
        {
            b.BackColor = Color.Red;
            b.ForeColor = Color.White;
        }

        public void botaoNaoDestacarPersonagens(System.Windows.Forms.Button b)
        {
            b.BackColor = Color.FromArgb(9, 18, 23);
            b.ForeColor = Color.DeepSkyBlue;
        }

        public void botaoNaoDestacarconfiguracao(System.Windows.Forms.Button b)
        {
            b.BackColor = Color.DeepSkyBlue;
            b.ForeColor = Color.FromArgb(9, 18, 23);
        }

        
        public void debug(string texto)
        {
            debug(texto, true, false, false, false, false);
        }

        public void debug(string texto, bool recordGeralDebugFile)
        {
            debug(texto, recordGeralDebugFile, false, false, false, false);
        }

        public void debug(string texto, bool recordGeralDebugFile, bool showAdministrator)
        {
            debug(texto, recordGeralDebugFile, showAdministrator, false, false, false);
        }

        public void debug(string texto, bool recordGeralDebugFile, bool showAdministrator, bool recordAlgoritmoAutoFile)
        {
            debug(texto, recordGeralDebugFile, showAdministrator, recordAlgoritmoAutoFile, false, false);
        }
        public void debug(string texto, bool recordGeralDebugFile, bool showAdministrator,bool recordAlgoritmoAutoFile,bool recordRetornoServidorFile,bool recordVariablesStatusFile)
        {
            


            string nomejogador="anonymous";
            if (_meuCodigoJogador > -1)
            {
                nomejogador = _jogadoresNome[_meuCodigoJogador];
            }
            if (recordGeralDebugFile)
                debuginfile(texto, "GeneralDebug."+nomejogador);
            if (showAdministrator)
                txtdebug.Text = "\r\n" + Convert.ToString(_counterDebug) + "-" + texto + txtdebug.Text;
            if (recordAlgoritmoAutoFile)
                debuginfile(texto, "AlgoritmoDebug." + nomejogador);
            if (recordRetornoServidorFile)
                debuginfile(texto, "RetornoServidor." + nomejogador);
            if(recordVariablesStatusFile)
                debuginfile(texto, "VariaveisStatus." + nomejogador);

        }



        public void debuginfile(string texto, string nomeadicionaldoarquivo)
        {
            if (!_ativarDebug) return;

            if (!Directory.Exists(debugfilepath))
            {

                //Criamos um com o nome folder
                Directory.CreateDirectory(debugfilepath);

            }



            try
            {
                debugfile = File.AppendText(debugfilepath + "/"+ nomeadicionaldoarquivo+ Convert.ToString(debugfilecode)+".txt");
            }
            catch (Exception e)//
            {
                //Caso nao consiga acessar o arquivo cria um novo arquivo debug
                //MessageBox.Show(e.Message);
                debugfile = File.AppendText(debugfilepath + "/" + nomeadicionaldoarquivo + Convert.ToString(++debugfilecode) + ".txt");
            }
            debugfile.WriteLine(Convert.ToString(++_counterDebug) + "-" + texto);
            debugfile.Close();
        }

        public void debugVariaveisGlobais(string rotulo)
        {
            rotulo = "\r\n\r\n************PrintVariaveisGlobais - " + rotulo + "****************************\r\n";
            string texto = "";
            //PROPRIEDADES
            texto += "_myId=" + _myId + "\r\n";
            texto += "_mySenha=" + _mySenha + "\r\n";
            texto += "_partidaId=" + _partidaId + "\r\n";
            texto += "_partidaSenha=" + _partidaSenha + "\r\n";
            //FLAGS DE CONTROLE
            texto += "\r\n";
            texto += "_partidaStatus=" + _partidaStatus + "\r\n";
            texto += "_rotinaPartidaIniciada=" + _rotinaPartidaIniciada + "\r\n";
            texto += "_autojogo=" + _autojogo + "\r\n";
            texto += "_nrodadaAtual=" + _nrodadaAtual + "\r\n";
            texto += "_primeirajogadarealizada=" + _primeirajogadarealizada + "\r\n";
            texto += "_candidato=" + _candidato + "\r\n";
            //PONTOS
            texto += "\r\n";
            texto += "_pontuacaoAtual=" + _pontuacaoAtual + "\r\n";
            texto += "_pontuacaoAtual=" + _pontuacaoTotal + "\r\n";
            //JOGADORES
            int i = 0;
            texto += "\r\n";
            texto += "_meuCodigoJogador=" + _meuCodigoJogador + "\r\n";
            texto += "_nJogadores=" + _nJogadores + "\r\n";
            texto += "_nCartasNaoMax=" + _nCartasNaoMax + "\r\n";
            texto += "Jogadores[id pontos cartasNAO HistoricoVotos]" + "\r\n";
            while (i < 6)
            {
                texto += _jogadoresId[i] + "\t";
                texto += _jogadoresNome[i] + "\t";
                texto += _jogadoresPontos[i] + "\t";
                texto += _jogadoresCartasNaoDiponiveis[i] + "\t";

                texto += _jogadoresHistoricoVotos[i] + "\t";
                texto += _jogadoresUltimaJogadaAnalisada[i] + "\t";
                texto += _jogadoresJogadacontador[i] + "\t";
                texto += "\r\n";
                i++;
            }
            //PERSONAGENS
            texto += "\r\n";
            texto += "_nPersonagens=" + _nPersonagens + "\r\n";
            i = 0;
            texto += "Personagens [id Status Meu Codinome]= " + "\r\n";
            while (i < 13)
            {
                texto += i + "\t";
                texto += _personagemStatus[i] + "\t";
                texto += _personagemMeu[i] + "\t";
                texto += _personagemCodinome[i] + "\t";
                texto += _personagemNome[i] + "\t";
                texto += "\r\n";
                i++;
            }

            i = 0;
            texto += "NivelqtdPersonagens = " + "\r\n";
            while (i < 11)
            {
                texto += i + "\t";
                texto += _nivelQtdPersonagens[i];
                texto += "\r\n";
                i++;
            }

            texto += "JogadoresCartas /Contadopromocoes= " + "\r\n";
            i = 0;
            int j = 0;
            while (i < 6)
            {
                j = 0;
                while (j < 13)
                {
                    texto += _jogadoresCartas[i, j] + "\t";
                    texto += _contadorPromocaoJogadorPersonagem[i, j] + "\t";
                    j++;
                }
                texto += "\r\n";
                i++;
            }
            texto += "\r\n";


            if (_debuginfileLastVariaveisGlobais == texto)
            {
                texto = "NoModifications";
            }
            else
            {
                _debuginfileLastVariaveisGlobais = texto;
            }

            debug(rotulo, false, false, false, false, true);
            debug(texto,false,false,false,false,true);

        }

        public void debugTabuleiro()
        {
            debug("debugTabuleiro()");
            string texto = "\r\nDISPOSICAO DO TABULEIRO\r\n" + n10.Text + (calculoMeuPersonagem(n10.Text) ? ("*") : ("")) + "\r\n";
            texto += n51.Text + (calculoMeuPersonagem(n51.Text) ? ("*") : ("")) + "\t" + n52.Text + (calculoMeuPersonagem(n52.Text) ? ("*") : ("")) + "\t" + n53.Text + (calculoMeuPersonagem(n53.Text) ? ("*") : ("")) + "\t" + n54.Text + (calculoMeuPersonagem(n54.Text) ? ("*") : ("")) + "\r\n";
            texto += n41.Text + (calculoMeuPersonagem(n41.Text) ? ("*") : ("")) + "\t" + n42.Text + (calculoMeuPersonagem(n42.Text) ? ("*") : ("")) + "\t" + n43.Text + (calculoMeuPersonagem(n43.Text) ? ("*") : ("")) + "\t" + n44.Text + (calculoMeuPersonagem(n44.Text) ? ("*") : ("")) + "\r\n";
            texto += n31.Text + (calculoMeuPersonagem(n31.Text) ? ("*") : ("")) + "\t" + n32.Text + (calculoMeuPersonagem(n32.Text) ? ("*") : ("")) + "\t" + n33.Text + (calculoMeuPersonagem(n33.Text) ? ("*") : ("")) + "\t" + n34.Text + (calculoMeuPersonagem(n34.Text) ? ("*") : ("")) + "\r\n";
            texto += n21.Text + (calculoMeuPersonagem(n21.Text) ? ("*") : ("")) + "\t" + n22.Text + (calculoMeuPersonagem(n22.Text) ? ("*") : ("")) + "\t" + n23.Text + (calculoMeuPersonagem(n23.Text) ? ("*") : ("")) + "\t" + n24.Text + (calculoMeuPersonagem(n24.Text) ? ("*") : ("")) + "\r\n";
            texto += n11.Text + (calculoMeuPersonagem(n11.Text) ? ("*") : ("")) + "\t" + n12.Text + (calculoMeuPersonagem(n12.Text) ? ("*") : ("")) + "\t" + n13.Text + (calculoMeuPersonagem(n13.Text) ? ("*") : ("")) + "\t" + n14.Text + (calculoMeuPersonagem(n14.Text) ? ("*") : ("")) + "\r\n";
            texto += n01.Text + (calculoMeuPersonagem(n01.Text) ? ("*") : ("")) + "\t" + n02.Text + (calculoMeuPersonagem(n02.Text) ? ("*") : ("")) + "\t" + n03.Text + (calculoMeuPersonagem(n03.Text) ? ("*") : ("")) + "\t" + n04.Text + (calculoMeuPersonagem(n04.Text) ? ("*") : ("")) + "\r\n";
            _calculoMinhaPontuacaoAtual = calculoMinhaPontuacaoAtual();
            _calculoMinhaPontuacaoTotalAtual = calculoMinhaPontuacaoTotalAtual();
            _calculoMaiorPontuacaoPossivelAtual = calculoMaiorPontuacaoPossivelAtual();
            _calculoMaiorPontuacaoTotalAtualPossivel = calculoMaiorPontuacaoTotalAtualPossivel();
            _calculoMinhaPontuacaoeBoa = calculoMinhaPontuacaoeBoa();
            _calculoQtdCartasNaoQueNaoSaoMinhas = calculoQtdCartasNaoQueNaoSaoMinhas();
            _calculoQtdMediaCartasNao = calculoQtdMediaCartasNao();
            _calculoQtdTotalAtualCartasNaoNoJogo = calculoQtdTotalAtualCartasNaoNoJogo();




            texto += "MinhaPontuacaoAtual = " + _calculoMinhaPontuacaoAtual + "\r\n";
            texto += "MinhaPontuacaoTotal = " + _jogadoresPontos[_meuCodigoJogador] + "\r\n";
            texto += "MinhaPontuacaoTotalAtual = " + _calculoMinhaPontuacaoTotalAtual + "\r\n";
            texto += "MaiorPontuacaoPossivelAtual =" + _calculoMaiorPontuacaoPossivelAtual + "\r\n";
            texto += "MaiorPontuacaoTotalAtualPossivel =" + _calculoMaiorPontuacaoTotalAtualPossivel + "\r\n";
            texto += "MinhaPontuacaoeBoa =" + _calculoMinhaPontuacaoeBoa + "\r\n";
            texto += "QtdCartasNaoQueNaoSaoMinhas =" + _calculoQtdCartasNaoQueNaoSaoMinhas + "\r\n";
            texto += "QtdMediaCartasNao =" + _calculoQtdMediaCartasNao + "\r\n";
            texto += "QtdTotalAtualCartasNaoNoJogo =" + _calculoQtdTotalAtualCartasNaoNoJogo + "\r\n";
            texto += "\r\n";
            debug(texto, false, false, true);
        }

        public string debugtextoJogadoresCartas()
        {
            string texto = "JogadoresCartas /Contadopromocoes= " + "\r\n";
            int i = 0;
            int j = 0;
            while (i < 6)
            {
                j = 0;
                while (j < 13)
                {
                    texto += _jogadoresCartas[i, j] + "\t";
                    texto += _contadorPromocaoJogadorPersonagem[i, j] + "\t";
                    j++;
                }
                texto += "\r\n";
                i++;
            }
            texto += "\r\n";
            return texto;
        }



        public bool calculoMeuPersonagem(string personagem)
        {
            if (personagem == "") return false;
            if (_personagemMeu[getPersonagemIndex(personagem)])return true;
            return false;
        }

        

        public int getCandidato()
        {
            //retorna codigo do personagem que esta se candidatando a Presidencia
            int i = 0;
            foreach (int nivel in _personagemStatus)
            {
                if (nivel == 10) return i;
                i++;
            }
            return -1;
        }

        public int getJogadorIndex(string nome)
        {
            int i = 0;
            while (i < _jogadoresNome.Length)
            {
                if (_jogadoresNome[i].Equals(nome)) return i;
                i++;
            }
            debug("ERRO getJogadorIndex(): indice do jogador nao encontrado: " + nome,true,true);
            return 0;
        }

        public int getJogadorIndex(int id)
        {
            int i = 0;
            while (i < _jogadoresId.Length)
            {
                if (_jogadoresId[i].Equals(id)) return i;
                i++;
            }
            debug("ERRO getJogadorIndex(): indice do jogador nao encontrado: ",true,true);
            return 0;
        }

        public int getPersonagemIndex(string nome)
        {
            int i = 0;
            while (i < _personagemCodinome.Length)
            {
                if (_personagemCodinome[i].Equals(nome)) return i;
                i++;
            }
            debug("ERRO getPersonagemIndex(): indice do Personagem nao encontrado: " + nome, true, true);
            return 0;
        }

        private void inicializarCartasNao()
        {
            if (_nJogadores == 0)
            {
                debug("ERROR: inicializarCartasNao() - Numero de Jogadores Igual a Zero", true, true);
                return;
            }
            _nCartasNaoMax = _nPersonagens / _nJogadores;
            /* COnforme Manual
             * 3 jogadores = 4 cartas nao
             * 4 jogadores = 3 cartas nao
             * 5 jogadores = 2 cartas nao
             * 6 jogadores = 2 cartas nao
             * 
             * Ha uma situacao especial para somente 2 jogadores, que sao possiveis apenas para testes
             * 2 jogadores= 4 cartas nao, e nao 6
             */
            if (_nCartasNaoMax > 4) _nCartasNaoMax = 4;

            int i = 0;
            while (i < _nJogadores)
            {
                _jogadoresCartasNaoDiponiveis[i] = _nCartasNaoMax;
                _jogadoresHistoricoVotos[i] = "";
                _jogadoresUltimaJogadaAnalisada[i] = "";
                _jogadoresJogadacontador[i] = 0;
                i++;
            }

            return;

        }

        public void inicializarVariaveisGlobais()
        {


            _myId = -1;
            _mySenha = "";
            _partidaSenha = "";
            _partidaId = 0;

            _partidaStatus = 0;
            _rotinaPartidaIniciada = false;
            //_autojogo = false;
            //_TimerHabilitado = false;
            _nrodadaAtual = 0;
            _primeirajogadarealizada = false;
            _candidato = -1;

            _pontuacaoAtual = 0;
            _pontuacaoTotal = 0;

            _nJogadores = 1;
            _nCartasNaoMax = 0;
            _meuCodigoJogador = 0;
            int i = 0;
            while (i < _jogadoresId.Length)
            {
                _jogadoresId[i] = 0;
                _jogadoresNome[i] = "";
                _jogadoresPontos[i] = 0;
                _jogadoresCartasNaoDiponiveis[i] = 0;
                _jogadoresHistoricoVotos[i] = "";
                _jogadoresUltimaJogadaAnalisada[i] = "";
                _jogadoresJogadacontador[i] = 0;
                i++;
            }
            _jogadorQueCandidatou = -1;

            _algoritmoPromocao = (int)ALGORITMO.promocao1Padrao;
            _algoritmoPosicionamento = (int)ALGORITMO.posicionamento0Padrao;
            _algoritmoVotacao = (int)ALGORITMO.votacao2Padrao;
            _algoritmoPromocaoPrioridadeAtual = 0;

        }
        public void inicializarNiveis()
        {
            int i = 0;
            while (i < _nivelQtdPersonagens.Length)
            {
                _nivelQtdPersonagens[i] = 0;
                i++;
            }
        }
        public void inicializarJogadoresCartas()
        {
            int i = 0;
            int j = 0;
            while (i < 6)
            {
                j = 0;
                while (j < 13)
                {
                    _jogadoresCartas[i, j] = 1;
                    _contadorPromocaoJogadorPersonagem[i, j] = 0;
                    j++;
                }
                i++;
            }
        }

        public void jogadoresLimparPainel()
        {
            lblPontos0.Visible = false;
            lblPontos1.Visible = false;
            lblPontos2.Visible = false;
            lblPontos3.Visible = false;
            lblPontos4.Visible = false;
            lblPontos5.Visible = false;
            lblJogadas0.Text = "";
            lblJogadas1.Text = "";
            lblJogadas2.Text = "";
            lblJogadas3.Text = "";
            lblJogadas4.Text = "";
            lblJogadas5.Text = "";
            //lblJogadas0.Visible = false;
            //lblJogadas1.Visible = false;
            //lblJogadas2.Visible = false;
            //lblJogadas3.Visible = false;
            //lblJogadas4.Visible = false;
            //lblJogadas5.Visible = false;

            labelVotacao0.Text = "";
            labelVotacao1.Text = "";
            labelVotacao2.Text = "";
            labelVotacao3.Text = "";
            labelVotacao4.Text = "";
            labelVotacao5.Text = "";
            labelVotacao0.Visible = false;
            labelVotacao1.Visible = false;
            labelVotacao2.Visible = false;
            labelVotacao3.Visible = false;
            labelVotacao4.Visible = false;
            labelVotacao5.Visible = false;



            j0.Visible = false;
            j1.Visible = false;
            j2.Visible = false;
            j3.Visible = false;
            j4.Visible = false;
            j5.Visible = false;
            nao00.Visible = false;
            nao01.Visible = false;
            nao02.Visible = false;
            nao03.Visible = false;
            nao10.Visible = false;
            nao11.Visible = false;
            nao12.Visible = false;
            nao13.Visible = false;
            nao20.Visible = false;
            nao21.Visible = false;
            nao22.Visible = false;
            nao23.Visible = false;
            nao30.Visible = false;
            nao31.Visible = false;
            nao32.Visible = false;
            nao33.Visible = false;
            nao40.Visible = false;
            nao41.Visible = false;
            nao42.Visible = false;
            nao43.Visible = false;
            nao50.Visible = false;
            nao51.Visible = false;
            nao52.Visible = false;
            nao53.Visible = false;
        }


        public void jogadoresMostrar()
        {
            int i = 0;
            string nome = "";
            string pontos = "";
            while (i < _nJogadores)
            {
                nome = _jogadoresNome[i];
                pontos = Convert.ToString(_jogadoresPontos[i]);
                switch (i)
                {
                    case 0:
                        j0.Text = nome;
                        j0.Visible = true;
                        lblPontos0.Text = pontos;
                        lblPontos0.Visible = true;
                        lblJogadas0.Visible = true;
                        //if (_meuCodigoJogador == 0)
                        //{
                        //    destacarBotao(j0);
                        //}
                        break;
                    case 1:
                        j1.Text = nome;
                        j1.Visible = true;
                        lblPontos1.Text = pontos;
                        lblPontos1.Visible = true;
                        lblJogadas1.Visible = true;
                        //if (_meuCodigoJogador == 1)
                        //{
                        //    destacarBotao(j1);
                        //}

                        break;
                    case 2:
                        j2.Text = nome;
                        j2.Visible = true;
                        lblPontos2.Text = pontos;
                        lblPontos2.Visible = true;
                        lblJogadas2.Visible = true;
                        //if (_meuCodigoJogador == 2)
                        //{
                        //    destacarBotao(j2);
                        //}

                        break;
                    case 3:
                        j3.Text = nome;
                        j3.Visible = true;
                        lblPontos3.Text = pontos;
                        lblPontos3.Visible = true;
                        lblJogadas3.Visible = true;
                        //if (_meuCodigoJogador == 3)
                        //{
                        //    destacarBotao(j3);
                        //}

                        break;
                    case 4:
                        j4.Text = nome;
                        j4.Visible = true;
                        lblPontos4.Text = pontos;
                        lblPontos4.Visible = true;
                        lblJogadas4.Visible = true;
                        //if (_meuCodigoJogador == 4)
                        //{
                        //    destacarBotao(j4);
                        //}

                        break;
                    case 5:
                        j5.Text = nome;
                        j5.Visible = true;
                        lblPontos5.Text = pontos;
                        lblPontos5.Visible = true;
                        lblJogadas5.Visible = true;
                        //if (_meuCodigoJogador == 5)
                        //{
                        //    destacarBotao(j5);
                        //}

                        break;
                }
                i++;
            }
           
        }

        public void jogadoresMostrarCartasNao(int jogador, int qtdcartas)
        {
            switch (jogador)
            {
                case 0:
                    if (qtdcartas > 0) { nao00.Visible = true; } else { nao00.Visible = false; }
                    if (qtdcartas > 1) { nao01.Visible = true; } else { nao01.Visible = false; }
                    if (qtdcartas > 2) { nao02.Visible = true; } else { nao02.Visible = false; }
                    if (qtdcartas > 3) { nao03.Visible = true; } else { nao03.Visible = false; }
                    break;
                case 1:
                    if (qtdcartas > 0) { nao10.Visible = true; } else { nao10.Visible = false; }
                    if (qtdcartas > 1) { nao11.Visible = true; } else { nao11.Visible = false; }
                    if (qtdcartas > 2) { nao12.Visible = true; } else { nao12.Visible = false; }
                    if (qtdcartas > 3) { nao13.Visible = true; } else { nao13.Visible = false; }
                    break;
                case 2:
                    if (qtdcartas > 0) { nao20.Visible = true; } else { nao20.Visible = false; }
                    if (qtdcartas > 1) { nao21.Visible = true; } else { nao21.Visible = false; }
                    if (qtdcartas > 2) { nao22.Visible = true; } else { nao22.Visible = false; }
                    if (qtdcartas > 3) { nao23.Visible = true; } else { nao23.Visible = false; }
                    break;
                case 3:
                    if (qtdcartas > 0) { nao30.Visible = true; } else { nao30.Visible = false; }
                    if (qtdcartas > 1) { nao31.Visible = true; } else { nao31.Visible = false; }
                    if (qtdcartas > 2) { nao32.Visible = true; } else { nao32.Visible = false; }
                    if (qtdcartas > 3) { nao33.Visible = true; } else { nao33.Visible = false; }
                    break;
                case 4:
                    if (qtdcartas > 0) { nao40.Visible = true; } else { nao40.Visible = false; }
                    if (qtdcartas > 1) { nao41.Visible = true; } else { nao41.Visible = false; }
                    if (qtdcartas > 2) { nao42.Visible = true; } else { nao42.Visible = false; }
                    if (qtdcartas > 3) { nao43.Visible = true; } else { nao43.Visible = false; }
                    break;
                case 5:
                    if (qtdcartas > 0) { nao50.Visible = true; } else { nao50.Visible = false; }
                    if (qtdcartas > 1) { nao51.Visible = true; } else { nao51.Visible = false; }
                    if (qtdcartas > 2) { nao52.Visible = true; } else { nao52.Visible = false; }
                    if (qtdcartas > 3) { nao53.Visible = true; } else { nao53.Visible = false; }
                    break;
            }
        }
        public void jogadoresMostrarVotacao(int jogador, string escolhaVoto)
        {
            switch (jogador)
            {
                case 0:
                    labelVotacao0.Text = escolhaVoto;
                    labelVotacao0.Visible = true;
                    break;
                case 1:
                    labelVotacao1.Text = escolhaVoto;
                    labelVotacao1.Visible = true;
                    break;
                case 2:
                    labelVotacao2.Text = escolhaVoto;
                    labelVotacao2.Visible = true;
                    break;
                case 3:
                    labelVotacao3.Text = escolhaVoto;
                    labelVotacao3.Visible = true;
                    break;
                case 4:
                    labelVotacao4.Text = escolhaVoto;
                    labelVotacao4.Visible = true;
                    break;
                case 5:
                    labelVotacao5.Text = escolhaVoto;
                    labelVotacao5.Visible = true;
                    break;
            }
            _jogadorQueCandidatou = -1;
        }

        public void jogadoresLimparVotacao(int jogadorQueCandidatou)
        {
            _jogadorQueCandidatou = jogadorQueCandidatou;

            labelVotacao0.Text = "";
            labelVotacao1.Text = "";
            labelVotacao2.Text = "";
            labelVotacao3.Text = "";
            labelVotacao4.Text = "";
            labelVotacao5.Text = "";
            
            switch (jogadorQueCandidatou)
            {
                case 0:
                    labelVotacao0.Text = "CANDIDATOU";
                    labelVotacao0.Visible = true;
                    break;
                case 1:
                    labelVotacao1.Text = "CANDIDATOU";
                    labelVotacao1.Visible = true;
                    break;
                case 2:
                    labelVotacao2.Text = "CANDIDATOU";
                    labelVotacao2.Visible = true;
                    break;
                case 3:
                    labelVotacao3.Text = "CANDIDATOU";
                    labelVotacao3.Visible = true;

                    break;
                case 4:
                    labelVotacao4.Text = "CANDIDATOU";
                    labelVotacao4.Visible = true;
                    break;
                case 5:
                    labelVotacao5.Text = "CANDIDATOU";
                    labelVotacao5.Visible = true;
                    break;
            }
        }
        public int getJogadorAnterior(int jogador)
        {
            int jogadoranterior = jogador-1;
            if (jogadoranterior < 0) jogadoranterior = _nJogadores - 1;
            return jogadoranterior;
        }
        public void mostrarQueVotou(int jogador)
        {
            switch (jogador)
            {
                case 0:
                    labelVotacao0.Text = "VOTOU";
                    labelVotacao0.Visible = true;
                    break;
                case 1:
                    labelVotacao1.Text = "VOTOU";
                    labelVotacao1.Visible = true;
                    break;
                case 2:
                    labelVotacao2.Text = "VOTOU";
                    labelVotacao2.Visible = true;
                    break;
                case 3:
                    labelVotacao3.Text = "VOTOU";
                    labelVotacao3.Visible = true;
                    break;
                case 4:
                    labelVotacao4.Text = "VOTOU";
                    labelVotacao4.Visible = true;
                    break;
                case 5:
                    labelVotacao5.Text = "VOTOU";
                    labelVotacao5.Visible = true;
                    break;
            }
        }

        public void jogadoresMostrarQueVotou(int jogadordavez)
        {
            jogadordavez = getJogadorIndex(jogadordavez);
            int jogadorAnterior = getJogadorAnterior(jogadordavez);
            while (jogadorAnterior != _jogadorQueCandidatou)
            {
                mostrarQueVotou(jogadorAnterior);
                jogadorAnterior = getJogadorAnterior(jogadorAnterior);
            }
        }
        private void jogadoresDestacar(int jogadordaVez, int meucodigoJogador)
        {
            int i = 0;
            while (i < _nJogadores)
            {
                switch (i)
                {
                    case 0:

                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacarJogadordaVez(j0); }
                        else if (meucodigoJogador == i) { botaoDestacarMeuPersonagem(j0); }
                        else { botaoNaoDestacarPersonagens(j0); }
                        break;
                    case 1:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacarJogadordaVez(j1); }
                        else if (meucodigoJogador == i) { botaoDestacarMeuPersonagem(j1); }
                        else { botaoNaoDestacarPersonagens(j1); }
                        break;
                    case 2:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacarJogadordaVez(j2); }
                        else if (meucodigoJogador == i) { botaoDestacarMeuPersonagem(j2); }
                        else { botaoNaoDestacarPersonagens(j2); }
                        break;
                    case 3:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacarJogadordaVez(j3); }
                        else if (meucodigoJogador == i) { botaoDestacarMeuPersonagem(j3); }
                        else { botaoNaoDestacarPersonagens(j3); }
                        break;
                    case 4:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacarJogadordaVez(j4); }
                        else if (meucodigoJogador == i) { botaoDestacarMeuPersonagem(j4); }
                        else { botaoNaoDestacarPersonagens(j4); }
                        break;
                    case 5:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacarJogadordaVez(j5); }
                        else if (meucodigoJogador == i) { botaoDestacarMeuPersonagem(j5); }
                        else { botaoNaoDestacarPersonagens(j5); }
                        break;
                }
                jogadoresMostrarCartasNao(i, _jogadoresCartasNaoDiponiveis[i]);
                i++;
            }
        }
        private void listarPartidas()
        {
            lstPartidas.Items.Clear();

            string texto = servidor(0);

            if (texto == null)
            {
                debug("ERROR: Listar Partidas Retornou null",true,true);
                return;
            }

            texto = texto.Replace("\r", "");

            string[] partidas = texto.Split('\n');

            try
            {
                ListViewItem item;

                foreach (string partida in partidas)
                {
                    if (partida != "")
                    {
                        string[] itens = partida.Split(',');
                        item = new ListViewItem();
                        item.Text = itens[1];

                        item.SubItems.Add(itens[3]);
                        if (itens[0] == "J")
                            item.SubItems.Add("Jogando");
                        else
                            item.SubItems.Add("Aberto");

                        lstPartidas.Items.Add(item);


                        if (itens[3].Equals("Londres")&& itens[0] == "A")
                        {
                            lblIdPartida.Text = itens[1];
                            txtSenhadaPartidaEntrar.Text = "123";
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro durante a execução da instrução.\nErro : " + ex.Message, "C#", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listarCartas()
        {

            string result = servidor(3);

            if (result == null) return;

            int ncartas = result.Length - 2;
            //txtCartas.Text = result;

            int i = 0;
            int j = 0;
            while (i < ncartas)
            {
                string minhacarta = result.Substring(i, 1);
                while (j < _personagemCodinome.Length)
                {
                    if (_personagemCodinome[j] == minhacarta)
                    {
                        _personagemMeu[j] = true;
                        j++;
                        break;
                    }
                    else
                    {
                        _personagemMeu[j] = false;
                    }
                    j++;
                }
                i++;
            }
        }
        private void listarSetores()
        {
            /*
            //listSetores.Items.Clear();

            string result = servidor(5);

            if (result == null) return;

            string texto = result.Substring(0, result.Length - 2);

            string[] setores = texto.Split('\r');

            foreach (String setor in setores)
            {
                //ListViewItem item = new ListViewItem();

                string setor2 = setor.Replace('\n', ' ');

                string[] campos = setor2.Split(',');

                //item.Text = setor2;
                //listSetores.Items.Add(item);

                int idsetor = Convert.ToInt32(campos[0]);

                switch (idsetor)
                {
                    case 0:
                        labelnivel0.Text = setor2;
                        break;
                    case 1:
                        labelnivel1.Text = setor2;
                        break;
                    case 2:
                        labelnivel2.Text = setor2;
                        break;
                    case 3:
                        labelnivel3.Text = setor2;
                        break;
                    case 4:
                        labelnivel4.Text = setor2;
                        break;
                    case 5:
                        labelnivel5.Text = setor2;
                        break;
                    case 10:
                        labelnivel6.Text = setor2;
                        break;
                }
            }*/

        }
        private int listarPersonagens()
        {

            //listPersonagens.Items.Clear();

            string result = servidor(4);

            if (result == null) return 0;

            string texto = result.Substring(0, result.Length - 2);

            string[] personagens = texto.Split('\r');

            int cdPersonagem = 0;
            foreach (String personagem in personagens)
            {
                //ListViewItem item = new ListViewItem();

                string personagem2 = personagem.Replace('\n', ' ');

                string[] campos = personagem2.Split(',');

                string nomepersonagem = campos[0].Trim();
                _personagemCodinome[cdPersonagem] = nomepersonagem.Substring(0, 1);
                _personagemNome[cdPersonagem] = nomepersonagem;
                _personagemMeu[cdPersonagem] = false;
                _personagemStatus[cdPersonagem] = -1;

                //item.Text = nomepersonagem;
                //item.SubItems.Add("DESEMPREGADO");//Status Personagem
                //listPersonagens.Items.Add(item);

                cdPersonagem++;

            }



            return cdPersonagem;

        }



        private int listarJogadores()
        {

            //listJogadores.Items.Clear();

            string result = servidor(2);

            if (result == null) return 0;


            string texto = result.Substring(0, result.Length - 2);
            string[] jogadores = texto.Split('\r');
            _meuCodigoJogador = -1;
            int numerodeJogadores = 0;
            foreach (String jogador in jogadores)
            {
                //ListViewItem item = new ListViewItem();

                string jogador2 = jogador.Replace('\n', ' ');

                string[] campos = jogador2.Split(',');

                string id = campos[0].Trim();
                string nome = campos[1].Trim();
                string pontos = campos[2].Trim();

                if (_myId == Convert.ToInt32(id))
                {
                    //item.BackColor = Color.Yellow;
                    _meuCodigoJogador = numerodeJogadores;

                }

                _jogadoresId[numerodeJogadores] = Convert.ToInt32(id);
                _jogadoresPontos[numerodeJogadores] = Convert.ToInt32(pontos);
                _jogadoresNome[numerodeJogadores] = nome;



                //mostrarJogador(numerodeJogadores, nome, pontos);



                //item.SubItems.Add(nome);

                //item.SubItems.Add("");
                //item.SubItems.Add("");
                //item.SubItems.Add("");
                //item.SubItems.Add(pontos);

                //item.Text = id;
                //listJogadores.Items.Add(item);


                numerodeJogadores++;
            }

            return numerodeJogadores;
        }
        private void partidaCriar()
        {
            string result = servidor(6);

            if (result == null) return;
            int x = Convert.ToInt32(result);

            debug("PARTIDA CRIADA = " + x);
            _mostrarPaineldeInformacoes = false;
            mostarOcultarInformacoes();
            txtNomedoJogador.Focus();
            listarPartidas();

        }
        private void partidaEntrar(int idPartida, string senhaPartida)
        {
            string result = servidor(7);

            if (result == null) return;


            string[] campos = result.Split(',');

            debug("ENTRAR PARTIDA("+idPartida+"-"+labelNomePartidaSelecionada.Text+") = " + campos[0] + "  " + campos[1]);

            //atualizar propriedades
            _partidaId = idPartida;
            _partidaSenha = senhaPartida;
            _myId = Convert.ToInt32(campos[0]);
            _mySenha = campos[1];

            rotinaPartidaEntrar();
        }

        private void rotinaPartidaEntrar()
        {
            _nJogadores = listarJogadores();
            jogadoresMostrar();
            //_TimerHabilitado = true;
            mostrarHabilitarTimer();
            novaRodada();
            rotinaPrincipal();
        }

        public void partidaIniciar()
        {
            string result = servidor(9);

            if (result == null) return;

            debug("partidaIniciar()=OK");

            rotinaPrincipal();
        }
        public void partidaSair()
        {
            inicio();
        }
        private bool personagemPosicionar()
        {
            string result = servidor(8);
            if (result == null) return false;
            debug("POSICIONAR PERSONAGEM OK - " + txtPersonagem.Text + txtSetor.Text,true,true,true,false,false);
            return true;
        }

        public bool personagemPromover()
        {
            string result = servidor(10);
            if (result == null) return false;
            
            debug("PROMOCAO PERSONAGEM OK - " + txtPersonagem.Text, true, true, true, false, false);
            return true;

        }

        public bool personagemVotar(int escolha)
        {

            int candidato = getCandidato();

            string result=null;
            if (escolha > 0)
            {
                result = servidor(11);
                if(result == null)//se nao deu para votar nao
                {
                    escolha = 0;
                }
            }
            if(escolha==0)
            {
                result = servidor(12);
            }

            if (result == null) return false;

            _candidato = candidato;

            debug("VOTACAO OK - " + ((escolha > 0) ? ("SIM") : ("NAO")), true, true, true, false, false);
            return true;

        }

        public void rotinaPrincipal()
        {
            debug("\r\n\r\n\r\n",true,false,true,true,true);

            if(_myId <0)
            {
                debug("rotinaPrincipal() - RETURN - myId<0 - Nao entrou na Partida ainda");
                return;
            }

            debug("\r\n\r\nrotinaPrincipal()");
            int status = analisarStatus();

            _algoritmoPromocaoPrioridadeAtual = 0;

            if (status < 0)
            {
                debug("rotinaPrincipal() - RETURN - status <0");
                return;
            }
            

            switch (status)
            {
                case (int)STATUS.PARTIDAABERTA:

                    rotinaPartidaAberta();
                    break;

                case (int)STATUS.POSICIONAR:
                    if (!_rotinaPartidaIniciada) rotinaPartidaIniciada();
                    if (verificarVez())
                    {
                        debugTabuleiro();
                        // controlesMinhaVez(status);
                        if (_autojogo)
                        {
                            
                            if (!autoposicionar(_algoritmoPosicionamento))
                            {
                               
                                debug("ERRO AO AUTOPOSICIONAR",true,true,true);
                            }
                            debugTabuleiro();

                        }
                    }
                    break;
                case (int)STATUS.PROMOVER:
                    if (!_rotinaPartidaIniciada) rotinaPartidaIniciada();
                    if (verificarVez())
                    {
                        debugTabuleiro();
                        //controlesMinhaVez(status);
                        if (_autojogo)
                        {
                            
                            if (!autopromover(_algoritmoPromocao))
                            {
                                debug("ERRO AO AUTOPROMOVER",true,true,true);
                            }
                            debugTabuleiro();

                        }
                    }

                    break;
                case (int)STATUS.VOTAR:
                    if (!_rotinaPartidaIniciada) rotinaPartidaIniciada();
                    if (verificarVez())
                    {
                        debugTabuleiro();
                        //controlesMinhaVez(status);
                        if (_autojogo)
                        {
                            
                            if (!autovotar(_algoritmoVotacao))
                            {
                                debug("ERRO AO AUTOVOTAR",true,true,true);
                            }
                            debugTabuleiro();


                        }
                    }
                    break;
                case (int)STATUS.PARTIDAENCERRADA:
                    debugTabuleiro();
                    rotinaPartidaEncerrada();
                    break;
                default:
                    debug("ERRO: rotina = Status Indefinido: " + status, true, true);
                    break;
            }
            debug("rotinaPrincipal() - FIM");
            debugVariaveisGlobais("FinalRotina");
            algoritmoMostrarDestacar(-1, -1, -1);
        }

        public void rotinaPartidaAberta()
        {
            _nJogadores = listarJogadores();
            if (_nJogadores < 1) return;


            jogadoresMostrar();
            if (_nJogadores > 1)
            {
                btnIniciarPartida.Enabled = true;
            }
            else
            {
                btnIniciarPartida.Enabled = false;
            }
            paineis((int)STATUS.PARTIDAABERTA);


        }
        public void rotinaPartidaEncerrada()
        {
            listarJogadores();
            jogadoresLimparPainel();
            jogadoresMostrar();
            //inicializarCartasNao();
            jogadoresDestacar(-1, _meuCodigoJogador);
            paineis((int)STATUS.PARTIDAABERTA);

            lblPontuacaoTotal.Text = Convert.ToString(Convert.ToInt32(lblPontuacaoTotal.Text) + Convert.ToInt32(lblPontuacaoAtual.Text));
            lblErro.Text = "Partida Encerrada!!";
            //timer1.Enabled = false;
            _TimerHabilitado = false;

            

            
        }
        public void rotinaPartidaIniciada()
        {
            _nJogadores = listarJogadores();
            jogadoresMostrar();
            novaRodada();
            listarSetores();
            btnIniciarPartida.Enabled = false;
            _rotinaPartidaIniciada = true;//evita que esta funcao seja chamada mais de uma vez
            lblNumRodada.Text = "1/3";
            paineis((int)STATUS.PARTIDAINICIADA);
        }

        public void tabuleiroLimpar()
        {
            n01.Text = "";
            botaoNaoDestacarPersonagens(n01);

            n02.Text = "";
            botaoNaoDestacarPersonagens(n02);
            n03.Text = "";
            botaoNaoDestacarPersonagens(n03);
            n04.Text = "";
            botaoNaoDestacarPersonagens(n04);

            n11.Text = "";
            botaoNaoDestacarPersonagens(n11);
            n12.Text = "";
            botaoNaoDestacarPersonagens(n12);
            n13.Text = "";
            botaoNaoDestacarPersonagens(n13);
            n14.Text = "";
            botaoNaoDestacarPersonagens(n14);

            n21.Text = "";
            botaoNaoDestacarPersonagens(n21);
            n22.Text = "";
            botaoNaoDestacarPersonagens(n22);
            n23.Text = "";
            botaoNaoDestacarPersonagens(n23);
            n24.Text = "";
            botaoNaoDestacarPersonagens(n24);

            n31.Text = "";
            botaoNaoDestacarPersonagens(n31);
            n32.Text = "";
            botaoNaoDestacarPersonagens(n32);
            n33.Text = "";
            botaoNaoDestacarPersonagens(n33);
            n34.Text = "";
            botaoNaoDestacarPersonagens(n34);

            n41.Text = "";
            botaoNaoDestacarPersonagens(n41);
            n42.Text = "";
            botaoNaoDestacarPersonagens(n42);
            n43.Text = "";
            botaoNaoDestacarPersonagens(n43);
            n44.Text = "";
            botaoNaoDestacarPersonagens(n44);

            n51.Text = "";
            botaoNaoDestacarPersonagens(n51);
            n52.Text = "";
            botaoNaoDestacarPersonagens(n52);
            n53.Text = "";
            botaoNaoDestacarPersonagens(n53);
            n54.Text = "";
            botaoNaoDestacarPersonagens(n54);

            n10.Text = "";
            botaoNaoDestacarPersonagens(n10);

            btnd0.Text = ""; botaoNaoDestacarPersonagens(btnd0);
            btnd1.Text = ""; botaoNaoDestacarPersonagens(btnd1);
            btnd2.Text = ""; botaoNaoDestacarPersonagens(btnd2);
            btnd3.Text = ""; botaoNaoDestacarPersonagens(btnd3);
            btnd4.Text = ""; botaoNaoDestacarPersonagens(btnd4);
            btnd5.Text = ""; botaoNaoDestacarPersonagens(btnd5);
            btnd6.Text = ""; botaoNaoDestacarPersonagens(btnd6);
            btnd7.Text = ""; botaoNaoDestacarPersonagens(btnd7);
            btnd8.Text = ""; botaoNaoDestacarPersonagens(btnd8);
            btnd9.Text = ""; botaoNaoDestacarPersonagens(btnd9);
            btnd10.Text = ""; botaoNaoDestacarPersonagens(btnd10);
            btnd11.Text = ""; botaoNaoDestacarPersonagens(btnd11);
            btnd12.Text = ""; botaoNaoDestacarPersonagens(btnd12);

        }
            private void tabuleiroAtualizar(string posicoes)
        {

            //debug("ATUALIZAR TABULEIRO - APAGAR POSICAO 10 INICIALMENTE");

            //eliminar posicao 10 anterior sempre porque é a unica que pode personagem eliminado
            int i = 0;
            while (i < _personagemStatus.Length)
            {
                if (_personagemStatus[i] == 10)
                {
                    _personagemStatus[i] = -2;
                    _nivelQtdPersonagens[10] = 0;
                }
                i++;
            }
            tabuleiroApagarPersonagem(10, "");


            //posicionar personagens
            if (posicoes.Length > 0)
            {
                string[] campos = posicoes.Split('\r');
                i = 0;
                while (i < campos.Length)
                {
                    string campos2 = campos[i].Replace('\n', ' '); // Substitui ocorrencias do caractere "\n" por " " (Vazio)
                    string[] personagemenivel = campos2.Split(',');

                    int nivel;
                    Int32.TryParse(personagemenivel[0].Trim(), out nivel);
                    tabuleiroPosicionarPersonagem(nivel, personagemenivel[1].Trim());
                    i++;
                }
            }
        }

        public void tabuleiroPosicionarDesempregados()
        {
            
            int personagem = 0;
            int status = 0;
            while (personagem < _personagemStatus.Length)
            {
                status = _personagemStatus[personagem];
                switch (personagem)
                {
                    case 0:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd0);
                            if (_personagemMeu[personagem]&&status<-1) botaoDestacarPersonagemEliminado(btnd0);
                            btnd0.Text = _personagemCodinome[personagem];
                        }
                        else
                        {

                            btnd0.Text = "";//* teste - apresentar nivel que meu personagem esta
                            //if (_personagemMeu[personagem]) btnd0.Text = Convert.ToString(status);
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd0);
                        }
                        break;
                    case 1:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd1);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd1);
                            btnd1.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd1.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd1);
                        }
                        break;
                    case 2:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd2);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd2);
                            btnd2.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd2.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd2);
                        }

                        break;
                    case 3:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd3);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd3);
                            btnd3.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd3.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd3);
                        }

                        break;
                    case 4:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd4);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd4);
                            btnd4.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd4.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd4);
                        }

                        break;
                    case 5:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd5);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd5);
                            btnd5.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd5.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd5);
                        }

                        break;
                    case 6:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd6);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd6);
                            btnd6.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd6.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd6);
                        }

                        break;
                    case 7:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd7);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd7);
                            btnd7.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd7.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd7);
                        }

                        break;
                    case 8:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd8);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd8);
                            btnd8.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd8.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd8);
                        }

                        break;
                    case 9:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd9);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd9);
                            btnd9.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd9.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd9);
                        }

                        break;
                    case 10:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd10);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd10);
                            btnd10.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd10.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd10);
                        }

                        break;
                    case 11:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd11);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd11);
                            btnd11.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd11.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd11);
                        }

                        break;
                    case 12:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacarMeuPersonagem(btnd12);
                            if (_personagemMeu[personagem] && status < -1) botaoDestacarPersonagemEliminado(btnd12);
                            btnd12.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd12.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacarPersonagens(btnd12);
                        }

                        break;
                }
                personagem++;
            }
        }
        public void tabuleiroPosicionarPersonagem(int nivel, string personagem)
        {
            //recuperar id do personagem
            int cdPersonagem = 0;
            while (cdPersonagem < _personagemCodinome.Length)
            {
                if (_personagemCodinome[cdPersonagem] == personagem)
                {
                    break;
                }
                cdPersonagem++;
                if (cdPersonagem == _personagemCodinome.Length)
                {
                    debug("ERRO FUNCAO: posicionarPersonagem(): cdPersonagem nao encontrado", true, true);
                    cdPersonagem = 0;
                    break;
                }
            }

            if (_personagemStatus[cdPersonagem] > -1)//Personagem ja foi posicionado alguma vez
            {
                if (_personagemStatus[cdPersonagem] == nivel)//personagem mantem a mesma posicao
                {
                    return;
                }
                else//personagem esta em outra posicao - apagar antiga posicao
                {
                    tabuleiroApagarPersonagem(_personagemStatus[cdPersonagem], personagem);
                    _nivelQtdPersonagens[_personagemStatus[cdPersonagem]]--;
                }
            }
            _personagemStatus[cdPersonagem] = nivel;
            _nivelQtdPersonagens[nivel]++;

            switch (nivel)
            {
                case 0:
                    if (n01.Text == "")
                    {
                        n01.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n01);
                    }
                    else if (n02.Text == "")
                    {
                        n02.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n02);
                    }
                    else if (n03.Text == "")
                    {
                        n03.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n03);
                    }
                    else if (n04.Text == "")
                    {
                        n04.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n04);
                    }
                    break;
                case 1:
                    if (n11.Text == "")
                    {
                        n11.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n11);
                    }
                    else if (n12.Text == "")
                    {
                        n12.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n12);
                    }
                    else if (n13.Text == "")
                    {
                        n13.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n13);
                    }
                    else if (n14.Text == "")
                    {
                        n14.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n14);
                    }
                    break;
                case 2:
                    if (n21.Text == "")
                    {
                        n21.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n21);
                    }
                    else if (n22.Text == "")
                    {
                        n22.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n22);
                    }
                    else if (n23.Text == "")
                    {
                        n23.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n23);
                    }
                    else if (n24.Text == "")
                    {
                        n24.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n24);
                    }

                    break;
                case 3:
                    if (n31.Text == "")
                    {
                        n31.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n31);
                    }
                    else if (n32.Text == "")
                    {
                        n32.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n32);
                    }
                    else if (n33.Text == "")
                    {
                        n33.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n33);
                    }
                    else if (n34.Text == "")
                    {
                        n34.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n34);
                    }

                    break;
                case 4:
                    if (n41.Text == "")
                    {
                        n41.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n41);
                    }
                    else if (n42.Text == "")
                    {
                        n42.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n42);
                    }
                    else if (n43.Text == "")
                    {
                        n43.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n43);
                    }
                    else if (n44.Text == "")
                    {
                        n44.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n44);
                    }

                    break;
                case 5:
                    if (n51.Text == "")
                    {
                        n51.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n51);
                    }
                    else if (n52.Text == "")
                    {
                        n52.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n52);
                    }
                    else if (n53.Text == "")
                    {
                        n53.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n53);
                    }
                    else if (n54.Text == "")
                    {
                        n54.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n54);
                    }

                    break;
                case 10:
                    n10.Text = personagem;
                    if (_personagemMeu[cdPersonagem]) botaoDestacarMeuPersonagem(n10);
                    break;
            }
        }

        public void tabuleiroApagarPersonagem(int nivel, string personagem)
        {

            switch (nivel)
            {
                case 0:
                    if (n01.Text == personagem)
                    {
                        n01.Text = "";
                        botaoNaoDestacarPersonagens(n01);

                    }
                    else if (n02.Text == personagem)
                    {
                        n02.Text = "";
                        botaoNaoDestacarPersonagens(n02);
                    }
                    else if (n03.Text == personagem)
                    {
                        n03.Text = "";
                        botaoNaoDestacarPersonagens(n03);
                    }
                    else if (n04.Text == personagem)
                    {
                        n04.Text = "";
                        botaoNaoDestacarPersonagens(n04);
                    }
                    break;
                case 1:
                    if (n11.Text == personagem)
                    {
                        n11.Text = "";
                        botaoNaoDestacarPersonagens(n11);
                    }
                    else if (n12.Text == personagem)
                    {
                        n12.Text = "";
                        botaoNaoDestacarPersonagens(n12);
                    }
                    else if (n13.Text == personagem)
                    {
                        n13.Text = "";
                        botaoNaoDestacarPersonagens(n13);
                    }
                    else if (n14.Text == personagem)
                    {
                        n14.Text = "";
                        botaoNaoDestacarPersonagens(n14);
                    }
                    break;
                case 2:
                    if (n21.Text == personagem)
                    {
                        n21.Text = "";
                        botaoNaoDestacarPersonagens(n21);
                    }
                    else if (n22.Text == personagem)
                    {
                        n22.Text = "";
                        botaoNaoDestacarPersonagens(n22);
                    }
                    else if (n23.Text == personagem)
                    {
                        n23.Text = "";
                        botaoNaoDestacarPersonagens(n23);
                    }
                    else if (n24.Text == personagem)
                    {
                        n24.Text = "";
                        botaoNaoDestacarPersonagens(n24);
                    }

                    break;
                case 3:
                    if (n31.Text == personagem)
                    {
                        n31.Text = "";
                        botaoNaoDestacarPersonagens(n31);
                    }
                    else if (n32.Text == personagem)
                    {
                        n32.Text = "";
                        botaoNaoDestacarPersonagens(n32);
                    }
                    else if (n33.Text == personagem)
                    {
                        n33.Text = "";
                        botaoNaoDestacarPersonagens(n33);
                    }
                    else if (n34.Text == personagem)
                    {
                        n34.Text = "";
                        botaoNaoDestacarPersonagens(n34);
                    }

                    break;
                case 4:
                    if (n41.Text == personagem)
                    {
                        n41.Text = "";
                        botaoNaoDestacarPersonagens(n41);
                    }
                    else if (n42.Text == personagem)
                    {
                        n42.Text = "";
                        botaoNaoDestacarPersonagens(n42);
                    }
                    else if (n43.Text == personagem)
                    {
                        n43.Text = "";
                        botaoNaoDestacarPersonagens(n43);
                    }
                    else if (n44.Text == personagem)
                    {
                        n44.Text = "";
                        botaoNaoDestacarPersonagens(n44);
                    }

                    break;
                case 5:
                    if (n51.Text == personagem)
                    {
                        n51.Text = "";
                        botaoNaoDestacarPersonagens(n51);
                    }
                    else if (n52.Text == personagem)
                    {
                        n52.Text = "";
                        botaoNaoDestacarPersonagens(n52);
                    }
                    else if (n53.Text == personagem)
                    {
                        n53.Text = "";
                        botaoNaoDestacarPersonagens(n53);
                    }
                    else if (n54.Text == personagem)
                    {
                        n54.Text = "";
                        botaoNaoDestacarPersonagens(n54);
                    }

                    break;
                case 10:
                    n10.Text = "";
                    botaoNaoDestacarPersonagens(n10);
                    break;
            }
        }

        //PARTE 2

        private void btnCriarPartida_Click(object sender, EventArgs e)
        {
            if (txtNomedaPartida.Text == "" || txtSenhaPartida.Text == "")
            {
                debug("Criacao de Partida Invalida!");
                return;
            }
            partidaCriar();
        }

        private void btnEntrarPartida_Click(object sender, EventArgs e)
        {
            int idpartida;
            if ((!Int32.TryParse(lblIdPartida.Text, out idpartida)) || (txtSenhadaPartidaEntrar.Text == "") || txtNomedoJogador.Text == "")
            {
                debugUser("Dados da Partida Inválidos",false);
                return;
            }
            partidaEntrar(idpartida, txtSenhadaPartidaEntrar.Text);
        }

        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            listarPartidas();
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {

        }

        

        private void btnPosicionar_Click(object sender, EventArgs e)
        {
            int setor;
            if (!Int32.TryParse(txtSetor.Text, out setor))
            {
                debug("SETOR INVALIDO");
                return;
            }
            if (txtPersonagem.Text == "")
            {
                debug("PERSONAGEM INVALIDO");
                return;
            }

            personagemPosicionar();
        }

        private void btnIniciarPartida_Click(object sender, EventArgs e)
        {
            partidaIniciar();
        }

        private void btnNao_Click(object sender, EventArgs e)
        {
            if (getCandidato() == -1)
            {
                debug("Nenhum candidato para Votacao");
                return;
            }

            personagemVotar(0);
        }

        private void btnPromover_Click(object sender, EventArgs e)
        {
            if(txtPersonagem.Text == "")
            {
                debug("personagem invalido");
                return;
            }
            personagemPromover();
        }

        private void btnSim_Click(object sender, EventArgs e)
        {
            if (getCandidato() == -1)
            {
                debug("Nenhum candidato para Votacao");
                return;
            }
            personagemVotar(1);
        }

        private void btnAlg3_Click(object sender, EventArgs e)
        {
            _algoritmoPosicionamento = (int)ALGORITMO.posicionamento3Equilibrado;
            algoritmoMostrarDestacar(-1,-1,-1);
        }

        private void btnAlg0_Click(object sender, EventArgs e)
        {
            _algoritmoPosicionamento = (int)ALGORITMO.posicionamento0Padrao;
            algoritmoMostrarDestacar(-1, -1, -1);
        }

        private void btnAlg1_Click(object sender, EventArgs e)
        {
            _algoritmoPromocao = (int)ALGORITMO.promocao1Padrao;
            algoritmoMostrarDestacar(-1, -1, -1);
        }

        private void btnAlg2_Click(object sender, EventArgs e)
        {
            _algoritmoVotacao = (int)ALGORITMO.votacao2Padrao;
            algoritmoMostrarDestacar(-1, -1, -1);
        }

        private void btnAlg4_Click(object sender, EventArgs e)
        {
            _algoritmoPromocao = (int)ALGORITMO.promocao4MeudeMenorNivel;
            algoritmoMostrarDestacar(-1, -1, -1);
        }

        private void btnAlg5_Click(object sender, EventArgs e)
        {
            _algoritmoVotacao = (int)ALGORITMO.votacao5TenhoPontosSuficientes;
            algoritmoMostrarDestacar(-1, -1, -1);
        }

        private void btnAlg7_Click(object sender, EventArgs e)
        {
            _algoritmoPromocao = (int)ALGORITMO.promocao7QualquerdeMenorNivel;
            algoritmoMostrarDestacar(-1, -1, -1);
        }

        public bool autoposicionar(int algoritmo)
        {
            debug("autoposicionar",true,false,true);
            //posicionar personagens nos niveis mais altos independente de serem meus ou nao em ordem alfabetica
            if (getCandidato() > -1)
            {
                debug("ERRO ALGORITMO: chamou posicionamento ao inves de votacao", true, true, true);
                return false;
            }

            if (calculoNumeroPersonagensDesempregados() < 1)
            {
                debug("ERRO ALGORITMO: chamou posicionamento ao inves de promocao", true, true, true);
                return false;
            }


            switch (algoritmo)
            {
                case (int)ALGORITMO.posicionamento0Padrao:
                    //algoritmoMostrarDestacar((int)ALGORITMO.posicionamento0Padrao, -1, -1);
                    if (algoritmoPosicionarPadrao()) return true;
                    debug("ERRO: algoritmo padrao nao funcionou", true, false, true);
                    break;
                case (int)ALGORITMO.posicionamento3Equilibrado:
                    //algoritmoMostrarDestacar((int)ALGORITMO.posicionamento3Equilibrado, -1, -1);
                    if (algoritmoPosicionamentoEquilibrado()) return true;
                    debug("ERRO: algoritmo posicionamento equilibrado nao funcionou", true, false, true);
                    return autoposicionar((int)ALGORITMO.posicionamento0Padrao);
            }
            return false;
        }




        public bool autopromover(int algoritmoPromocao)
        {
            debug("autopromover", true, false, true);
            //if (!esperar()) return;
            //promover personagens em ordem alfabetica independentende de serem meus ou nao
            if (getCandidato() > -1)
            {
                debug("ERRO ALGORITMO: chamou promocao ao inves de votacao", true, true, true);
                return false;
            }

            if (calculoNumeroPersonagensDesempregados() > 0)
            {
                debug("ERRO ALGORITMO: chamou promocao ao inves de posicionamento", true, true, true);
                return false;
            }


            if (_personagemEliminar > -1)
            {
                debug("autopromover: _personagem Eliminar = "+_personagemEliminar +"("+_personagemCodinome[_personagemEliminar]+")", true, false, true);
                if (algoritmoPromocaoPadrao(_personagemEliminar)) return true;
            }




            switch (algoritmoPromocao)
            {
                case (int)ALGORITMO.promocao1Padrao:
                    //algoritmoMostrarDestacar(-1, (int)ALGORITMO.promocao1Padrao, -1);
                    if (algoritmoPromocaoPadrao(-1)) return true;
                    debug("ERRO ALGORITMO: Promocao Padrao Nao funcionou", true, false, true);
                    return false;

                case (int)ALGORITMO.promocao10IniciarMeuReinado:
                    //algoritmoMostrarDestacar(-1, (int)ALGORITMO.promocao10IniciarMeuReinado, -1);
                    if (algoritmoPromocao9IniciarMeuReinado()) return true;
                    break;

                case (int)ALGORITMO.promocao4MeudeMenorNivel:
                    //simplesmente aumentar minha pontuacao
                    //algoritmoMostrarDestacar(-1, (int)ALGORITMO.promocao4MeudeMenorNivel, -1);
                    if (algoritmoPromocaoMeudeMenorNivel()) return true;
                    break;

                case (int)ALGORITMO.promocao13OutrosdeMaiorNivel:
                    //algoritmoMostrarDestacar(-1, (int)ALGORITMO.promocao13OutrosdeMaiorNivel, -1);
                    if (algoritmoPromocaoOutrosdeMaiorNivel()) return true;
                    break;

                case (int)ALGORITMO.promocao7QualquerdeMenorNivel:
                    //algoritmoMostrarDestacar(-1, (int)ALGORITMO.promocao7QualquerdeMenorNivel, -1);
                    if (algoritmoPromocaoQualquerdeMenorNivel()) return true;
                    break;

            }

            if ((_algoritmoPromocaoPrioridadeAtual+1) < _algoritmoPromocaoPrioridade.Length)
            {
                debug("ALGORITMO PRIORIDADE = "+ (_algoritmoPromocaoPrioridadeAtual + 1), true, false, true);
                return autopromover(_algoritmoPromocaoPrioridade[++_algoritmoPromocaoPrioridadeAtual]);
            }
            return false;

        }

        

        
        public bool autovotar(int algoritmo)
        {
            debug("autovotar", true, false, true);
            //if (!esperar()) return;
            //votar sim para meus personagens e nao para outros
            if (getCandidato() < 0)
            {
                debug("ERRO ALGORITMO: Não ha candidato para autovotar", true, true, true);
                return false;
            }

            if (_jogadoresCartasNaoDiponiveis[_meuCodigoJogador] < 1)
            {
                debug("Não tenho mais cartas Naos", true, false, true);
                return personagemVotar(1);
            }

            switch (algoritmo)
            {
                case (int)ALGORITMO.votacao2Padrao:
                    if (algoritmoVotacaoPadrao()) return true;
                    debug("ERRO ALGORTMO - algortimovotacaopadrao retornou falso", true, false, true);
                    break;

                case (int)ALGORITMO.votacao5TenhoPontosSuficientes:
                    if (algoritmoVotacaoTenhoPontosSuficientes()) return true;
                    debug("ERRO ALGORTMO - algoritmoVotacaoTenhoPontosSuficientes retornou falso", true, false, true);
                    if (algoritmoVotacao8EconomiadeNaos()) return true;
                    debug("ERRO ALGORTMO - algoritmoVotacao8EconomiadeNaos retornou falso", true, false, true);
                    return autovotar((int)ALGORITMO.votacao2Padrao);

            }
            return false;
        }
        public int calculoPersonagemOutrosdeMaiorNivel()
        {
            int personagem = 0;
            int nivel = 5;
            while (nivel > 3)
            {
                personagem = 0;
                while (personagem < _personagemStatus.Length)
                {
                    if (!_personagemMeu[personagem])
                    {
                        if (_personagemStatus[personagem] == nivel)
                        {
                            if (nivel == 5 && getCandidato() < 0)
                            {
                                debug("calculoPersonagemOutrosdeMaiorNivel(): ="+personagem, false, false, true);
                                return personagem;
                            }
                            else if (_nivelQtdPersonagens[nivel + 1] < 4)
                            {
                                debug("calculoPersonagemOutrosdeMaiorNivel(): =" + personagem, false, false, true);
                                return personagem;
                            }
                        }
                    }
                    personagem++;
                }
                nivel--;
            }
            debug("calculoPersonagemOutrosdeMaiorNivel(): Nenhum", false, false, true);
            return -1;
        }
        public int calculoNumeroPersonagensEliminados()
        {
            int soma = 0;
            int personagem = 0;
            while (personagem < _personagemStatus.Length)
            {
                if (_personagemStatus[personagem] == -2)//personagem eliminado
                {
                    soma++;
                }
                personagem++;
            }
            debug("calculoNumeroPersonagensEliminados=" + soma, false, false, true);
            return soma;
        }

        public int calculoNumeroPersonagensDesempregados()
        {
            int soma = 0;
            int personagem = 0;
            while (personagem < _personagemStatus.Length)
            {
                if (_personagemStatus[personagem] == -1)//personagem eliminado
                {
                    soma++;
                }
                personagem++;
            }
            debug("calculoNumeroPersonagensDesempregados=" + soma, false, false, true);
            return soma;

        }
        public int calculoQtdCartasNaoQueNaoSaoMinhas()
        {

            int i = 0;
            int soma = 0;
            while (i < _nJogadores)
            {
                if (_jogadoresCartasNaoDiponiveis[i] > 0 && i != _meuCodigoJogador)
                {
                    soma += _jogadoresCartasNaoDiponiveis[i];
                }
                i++;
            }

            debug("calculoQtdCartasNaoQueNaoSaoMinhas=" + soma, false, false, true);
            return soma;
        }

        public int calculoQtdTotalAtualCartasNaoNoJogo()
        {
            int jogador = 0;
            int soma = 0;
            while (jogador < _nJogadores)
            {
                soma += _jogadoresCartasNaoDiponiveis[jogador];
                jogador++;
            }
            debug("calculoQtdTotalAtualCartasNaoNoJogo=" + soma, false, false, true);
            return soma;
        }
        public int calculoPersonagemMeudeMaiorNivel()
        {
            debug("calculoPersonagemMeudeMaiorNivel", false, false, true);
            int nivel = 5;
            while (nivel >= 0)
            {
                int personagem = 0;
                while (personagem < _personagemStatus.Length)
                {
                    if (_personagemStatus[personagem] == nivel)
                    {
                        if (_personagemMeu[personagem])
                        {
                            debug("calculoPersonagemMeudeMaiorNivel=" + personagem + "(" + _personagemCodinome[personagem] + ")", false, false, true);
                            return personagem;
                        }
                    }
                    personagem++;
                }
                nivel--;
            }

            debug("calculoPersonagemdeMaiorNivel=INDEFINIDO", false, false, true);
            return -1;
        }

        public bool calculoPersonagemDesempregado(int personagem)
        {
            if (_personagemStatus[personagem] < 0) return true;
            return false;
        }

        public bool calculoNivelDisponivel(int nivel)
        {
            if (_nivelQtdPersonagens[nivel] < 4) return true;
            return false;
        }

        private int calculoMinhaPontuacaoAtual()
        {
            
            int i = 0;
            int pontos = 0;
            foreach (int status in _personagemStatus)
            {
                if (_personagemMeu[i] == true)
                {
                    if (status > 0)//numeros negativos nao entram na soma
                    {
                        pontos += status;
                    }

                }
                i++;
            }
            //debug("calculoMinhaPontuacaoAtual="+pontos, false, false, true);
            return pontos;
        }

        private int calculoMinhaPontuacaoTotalAtual()
        {
            int pontos = calculoMinhaPontuacaoAtual() + _jogadoresPontos[_meuCodigoJogador];
            //debug("calculoMinhaPontuacaoTotalAtual=" + pontos, false, false, true);
            return pontos;
        }

        public int calculoMaiorPontuacaoPossivelAtual()
        {
            //debug("calculoMaiorPontuacaoPossivelAtual()", false, false, true);
            int nivel = 5;
            int personagem = 0;
            int[] soma = new int[6];
            int contador = 0;
            int jogador = 0;
            int contadormax = 6;

            if (getCandidato() > -1)
            {
                contadormax = 5;
            }

            while (jogador < _nJogadores)
            {

                soma[jogador] = 0;
                contador = 0;
                if (jogador != _meuCodigoJogador)
                {
                    nivel = 5;
                    while (nivel > 0)
                    {
                        personagem = 0;
                        while (personagem < _personagemStatus.Length)
                        {
                            if (_jogadoresCartas[jogador, personagem] < 0)
                            {
                                contador++;//diminui as cartas do jogador
                            }
                            if (_personagemStatus[personagem] == nivel && _jogadoresCartas[jogador, personagem] > 0)
                            {

                                soma[jogador] += nivel;
                                contador++;
                                //MessageBox.Show("SOMOU "+_personagemCodinome[personagem]+"SOMA = "+soma);

                                if (contador == contadormax)
                                {
                                    //debug("calculoMaiorPontuacaoPossivelAtual() - Jogador "+jogador+"("+_jogadoresNome[jogador]+") = "+soma[jogador],false,false,true);
                                    break;
                                }
                                //somou os cinco maiores

                            }
                            personagem++;
                        }
                        if (contador == contadormax) break;

                        nivel--;
                    }

                }
                jogador++;
            }

            jogador = 0;
            int somaMaior = 0;
            while (jogador < _nJogadores)
            {
                if (soma[jogador] > somaMaior)
                {
                    somaMaior = soma[jogador];
                }
                jogador++;
            }
            
            if (getCandidato() > -1)
            {
                somaMaior += 10;
            }
            //debug("calculoMaiorPontuacaoPossivelAtual=" + somaMaior, false, false, true);
            return somaMaior; //soma ao presidente
        }

        public int calculoQtdMeusPersonagensNoNivel(int nivel)
        {
            int i = 0;
            int contador = 0;
            while (i < _personagemStatus.Length)
            {
                if (_personagemStatus[i] == nivel && _personagemMeu[i])
                {
                    contador++;
                }
                i++;
            }
            debug("calculoQtdMeusPersonagensNoNivel "+nivel+"="+contador, false, false, true);
            return contador;
        }
        public bool calculoMinhaPontuacaoeBoa()
        {
            debug("calculoMinhaPontuacaoeBoa()", false, false, true);
            
            

            if (_nrodadaAtual == 2)//o importante é termos a maior pontuacao
            {
                if (_calculoMaiorPontuacaoTotalAtualPossivel < _calculoMinhaPontuacaoTotalAtual)//Ninguem mais pode ganhar de mim
                {
                    debug("calculoMinhaPontuacaoeBoa = lider da partida - Mnha pontuacao Total Atual é maior que tudo", false, false, true);
                    return true;
                }
            }

            if ((_calculoMaiorPontuacaoPossivelAtual) <= (_calculoMinhaPontuacaoAtual * 0.8))//Segundo lugar tem menos de 80% dos meus pontos nesta rodada
            {
                debug("calculoMinhaPontuacaoeBoa = lider Oficial da rodada - maior pontuacao possivel < 80% dos meus pontos", false, false, true);
                return true;
            }

            if (_nrodadaAtual < 2)
            {
                if ((_calculoMaiorPontuacaoPossivelAtual * 0.75) <= (_calculoMinhaPontuacaoTotalAtual))//Se tenho 75% da maior pontuacao minha pontuacao é boa
                {
                    debug("calculoMinhaPontuacaoeBoa = uma boa rodada - nao é a ultima e tenho 75% da maior pontuacao possivel", false, false, true);
                    return true;
                }
            }
            debug("calculoMinhaPontuacaoeBoa = false", false, false, true);
            return false;
        }
        public int calculoMaiorPontuacaoTotalAtualPossivel()
        {
            //debug("calculoMaiorPontuacaoTotalAtualPossivel()", false, false, true);
            int i = 0;
            int maiorPontuacaoPossivelAtual = calculoMaiorPontuacaoPossivelAtual();
            int maiorPontuacaoTotalAtualPossivel = 0;
            while (i < _nJogadores)
            {
                if (i != _meuCodigoJogador)
                {
                    if ((_jogadoresPontos[i] + maiorPontuacaoPossivelAtual) > maiorPontuacaoTotalAtualPossivel)
                    {
                        maiorPontuacaoTotalAtualPossivel = _jogadoresPontos[i] + maiorPontuacaoPossivelAtual;
                    }
                    //debug("calculoMaiorPontuacaoTotalAtualPossivel()= jogador[" + i + "]("+_jogadoresNome[i]+")=" + (_jogadoresPontos[i] + maiorPontuacaoPossivelAtual), false, false, true);
                }
                i++;
            }

            return maiorPontuacaoTotalAtualPossivel;
        }

        public bool algoritmoPosicionarPadrao()
        {
            debug("algoritmoPosicionarPadrao", false, false, true);
            //posiciona os personagens nos nivens mais altos em ordem alfabetica
            int personagem = 0;
            int nivel = 4;
            while (personagem < _personagemStatus.Length)
            {
                if (calculoPersonagemDesempregado(personagem))
                {
                    //flagdesempregado = true;
                    while (nivel >= 1)
                    {
                        if (calculoNivelDisponivel(nivel))
                        {
                            txtPersonagem.Text = _personagemCodinome[personagem];
                            txtSetor.Text = Convert.ToString(nivel);

                            debug("algoritmoPosicionarPadrao - Tentar posicionar " + personagem + "(" + _personagemCodinome[personagem] + ")", false, false, true);

                            if (personagemPosicionar())
                            {
                                return true;
                            }
                            debug("algoritmoPosicionarPadrao - personagemPosicionar()=false", false, false, true);
                        }
                        nivel--;
                    }
                }
                personagem++;
            }
            //if (!flagdesempregado)
            //{
            //    debug("ERRO Nenhum personagem desempregado - FORCAR AUTOPROMOCAO");
            //    return autopromover(_algoritmoPromocao);
            //}
            return false;
        }



        public bool algoritmoPosicionamentoEquilibrado()
        {
            debug("algoritmoPosicionamentoEquilibrado()", false, false, true);
            int personagem = _personagemStatus.Length-1;
            while (personagem > -1 )
            {
                if (calculoPersonagemDesempregado(personagem))
                {
                    if (_personagemMeu[personagem])
                    {
                        int nivel = 4;
                        while (nivel > 0)
                        {
                            int qtdPersonagensMeusnoNivel = calculoQtdMeusPersonagensNoNivel(nivel);
                            if (qtdPersonagensMeusnoNivel < 2 && calculoNivelDisponivel(nivel))
                            {
                                if(nivel == 4 && qtdPersonagensMeusnoNivel > 0)//*posicionar apenas um personagem no nivel 4
                                {
                                    nivel--;
                                    continue;
                                }
                                txtPersonagem.Text = _personagemCodinome[personagem];
                                txtSetor.Text = Convert.ToString(nivel);
                                if (personagemPosicionar()) return true;
                            }
                            nivel--;
                        }
                    }
                }
                personagem--;
            }
            debug("algoritmoPosicionamentoEquilibrado()=false;", false, false, true);
            return false;
        }


        public bool algoritmoPromocaoPadrao(int personagemPrioritario)
        {

            debug("algoritmoPromocaoPadrao", false, false, true);
            //promove em ordem alfabetica
            int personagem = 0;
            if (personagemPrioritario > -1)
            {
                personagem = personagemPrioritario;
            }

            while (personagem < _personagemStatus.Length)
            {

                if (!calculoPersonagemDesempregado(personagem))
                {
                    if (_personagemStatus[personagem] == 5)
                    {
                        //promover para o nivel 10
                        txtPersonagem.Text = _personagemCodinome[personagem];
                        if (personagemPromover()) return true;
                    }
                    else if ((_nivelQtdPersonagens[_personagemStatus[personagem] + 1]) < 4)
                    {
                        txtPersonagem.Text = _personagemCodinome[personagem];

                        if (personagemPromover()) return true;
                    }
                }

                personagem++;
            }
            return false;
        }
        public bool algoritmoPromocaoMeudeMenorNivel()
        {
            debug("algoritmoPromocaoMeudeMenorNivel()", false, false, true);
            int personagem = 0;
            int nivel = 0;
            

            while (nivel < 4)//evita que eu promova personagem para o nivel 5
            {
                personagem = _personagemStatus.Length - 1;
                while (personagem >= 0)
                {
                    if (_personagemMeu[personagem] && _personagemStatus[personagem] > -1 && _personagemStatus[personagem]==nivel)
                    {
                        if ((_nivelQtdPersonagens[_personagemStatus[personagem] + 1]) < 4)
                        {
                            //if (calculoMinhaPontuacaoeBoa()&&)
                            //{
                            //    debug("algoritmoPromocaoMeudeMenorNivel() = false - PONTUACAO E BOA - NAO VOU PROMOVER O MEU", "tracert");
                            //    return false;//evita movimentos de meus personagens de niveis mais alto
                            //}


                            txtPersonagem.Text = _personagemCodinome[personagem];
                            debug("algoritmoPromocaoMeudeMenorNivel() = "+ txtPersonagem.Text, false, false, true);
                            if (personagemPromover()) return true;
                        }
                    }
                    personagem--;
                }
                nivel++;
            }
            debug("algoritmoPromocaoMeudeMenorNivel()=false", false, false, true);
            return false;
        }
        public int calculoQtdMediaCartasNao()
        {
            int qtdMediaCartasNao = (_nCartasNaoMax * (_nJogadores - 1) / 2);
            debug("calculoQtdMediaCartasNao() = "+qtdMediaCartasNao, false, false, true);
            return qtdMediaCartasNao;
        }

        public bool algoritmoPromocaoOutrosdeMaiorNivel()
        {
            debug("algoritmoPromocaoOutrosdeMaiorNivel()", false, false, true);
            
            
            if (_calculoQtdCartasNaoQueNaoSaoMinhas < _calculoQtdMediaCartasNao)//se cartas nao dos oponente sao poucas há um risco grande deste personagem qu eu eleger ser eleito
            {
                debug("algoritmoPromocaoOutrosdeMaiorNivel() = false - POUCAS CARTAS NAO NO JOGO - Muito arriscado promover oponente", false, false, true);
                return false;
            }


            int personagem = 0;
            int nivel = 5;
            while (nivel > 3)
            {
                personagem = 0;
                while (personagem < _personagemStatus.Length)
                {
                    if (_personagemStatus[personagem] > -1)
                    {
                        if (!_personagemMeu[personagem])
                        {
                            if ((calculoNivelDisponivel(_personagemStatus[personagem]+1)))
                            {
                                txtPersonagem.Text = _personagemCodinome[personagem];
                                debug("algoritmoPromocaoOutrosdeMaiorNivel() = "+personagem+"("+_personagemCodinome[personagem]+")", false, false, true);
                                if (personagemPromover()) return true;
                            }
                        }
                    }
                    personagem++;
                }
                nivel--;
            }
            debug("algoritmoPromocaoOutrosdeMaiorNivel() = false", false, false, true);
            return false;
        }

        public bool algoritmoPromocaoQualquerdeMenorNivel()
        {
            debug("algoritmoPromocaoQualquerdeMenorNivel()", false, false, true);
            int personagem = 0;
            int nivel = 0;
            while (nivel < 5)
            {
                personagem = 0;
                while (personagem < _personagemStatus.Length)
                {
                    if (_personagemStatus[personagem] > -1)
                    {
                        if ((_nivelQtdPersonagens[_personagemStatus[personagem] + 1]) < 4)
                        {
                            txtPersonagem.Text = _personagemCodinome[personagem];
                            debug("algoritmoPromocaoQualquerdeMenorNivel()="+ _personagemCodinome[personagem], false, false, true);
                            if (personagemPromover()) return true;
                        }
                    }
                    personagem++;
                }
                nivel++;
            }
            debug("algoritmoPromocaoQualquerdeMenorNivel()=false", false, false, true);
            return false;
        }

        public bool algoritmoPromocao9IniciarMeuReinado()
        {
            //passar a promover meu personagem de nivel mais alto ate a presidencia
            //somente os oponente nao mais tiverem cartas nao
            debug("algoritmoPromocao9IniciarMeuReinado()", false, false, true);

            int personagemOutrosdeMaiorNivel = calculoPersonagemOutrosdeMaiorNivel();
            int personagemMeudeMaiorNivel = calculoPersonagemMeudeMaiorNivel();

            if (calculoQtdCartasNaoQueNaoSaoMinhas() <= 0)
            {
                //se eu tenho cartas nao vou investir numa votacao em que eu vote Nao
                if (_jogadoresCartasNaoDiponiveis[_meuCodigoJogador] > 0)
                {
                    
                    //se diferenca entre os niveis dos dois personagens <2 passar a eliminar este personagem do outro
                    if (personagemOutrosdeMaiorNivel > -1)
                    {
                        if (_personagemStatus[personagemMeudeMaiorNivel] - _personagemStatus[personagemOutrosdeMaiorNivel] <2)
                        {
                            debug("algoritmoPromocao9IniciarMeuReinado(): Há um personagem para ser eliminado - "+ personagemOutrosdeMaiorNivel, false, false, true);
                            _personagemEliminar = personagemOutrosdeMaiorNivel;
                            if (algoritmoPromocaoPadrao(_personagemEliminar)) return true;
                        }
                    }
                }
                debug("algoritmoPromocao9IniciarMeuReinado(): Promover meu personagem de Maior Nivel - - Iniciar Reinado " + personagemMeudeMaiorNivel, false, false, true);
                if (algoritmoPromocaoPadrao(calculoPersonagemMeudeMaiorNivel())) return true;
            }
            return false;
        }


        public bool algoritmoVotacaoPadrao()
        {
            debug("algoritmoVotacaoPadrao()", false, false, true);
            int candidato = getCandidato();
            if (_personagemMeu[candidato] || (_jogadoresCartasNaoDiponiveis[_meuCodigoJogador] < 1))
            {
                return personagemVotar(1);
            }
            else
            {
                return personagemVotar(0);
            }
        }
        public bool algoritmoVotacaoTenhoPontosSuficientes()
        {

            if (_calculoMinhaPontuacaoeBoa)
            {
                debug("algoritmoVotacaoTenhoPontosSuficientes - VOTAR SIM", false, false, true);
                return personagemVotar(1);
            }
            debug("algoritmoVotacaoTenhoPontosSuficientes - false", false, false, true);
            return false;
        }

        public bool algoritmoVotacao8EconomiadeNaos()
        {
            debug("algoritmoVotacao8EconomiadeNaos()", false, false, true);
            
            if (_calculoQtdCartasNaoQueNaoSaoMinhas>_calculoQtdMediaCartasNao)//Se há uma quantidade elevada de Cartas Nao no jogo, Economizar Naos
            {
                debug("algoritmoVotacao8EconomiadeNaos - Há uma quantidade consideravel de votos Nao", false, false, true);
                if (_calculoMaiorPontuacaoTotalAtualPossivel*0.75<= _calculoMinhaPontuacaoTotalAtual)//e ainda tenho uma pontuacao relativamente boa
                {
                    debug("algoritmoVotacao8EconomiadeNaos - Tenho uma pontuacao consideravel - VOTAR SIM", false, false, true);
                    return personagemVotar(1);
                }
                else
                {
                    debug("algoritmoVotacao8EconomiadeNaos - Porem a maior pontuacao possivel é muito maior - Nao da para economizar Nao agora", false, false, true);
                }
            }
            debug("algoritmoVotacao8EconomiadeNaos()= false", false, false, true);
            return false;
        }

        
        

        
        public void eliminarCartaJogadoresCarta(int carta)
        {
            int i = 0;
            while (i < _nJogadores)
            {
                _jogadoresCartas[i, carta] = _jogadoresCartas[i, carta] * (-1);//inverte possibilidade
                i++;
            }
        }

        public void PartidaSelecionada()
        {
            if (lstPartidas.SelectedItems[0].SubItems[2].Text == "Jogando")
            {
                _mostrarPaineldeInformacoes = true;
                mostarOcultarInformacoes();
                MessageBox.Show("Erro: A partida não está aberta!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                lblIdPartida.Text = lstPartidas.SelectedItems[0].SubItems[0].Text;
                labelNomePartidaSelecionada.Text = lstPartidas.SelectedItems[0].SubItems[1].Text;
                _mostrarPaineldeInformacoes = false;
                mostarOcultarInformacoes();

                //MessageBox.Show("A Partida " + lstPartidas.SelectedItems[0].SubItems[1].Text + " foi selecionada com sucesso!\n\nInsira o nome do jogador e a senha da \npartida no canto inferior direito para \nentrar na partida.", "Partida selecionada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtNomedoJogador.Focus();
            }
        }

        private void lstPartidas_DoubleClick(object sender, EventArgs e)
        {
            PartidaSelecionada();
        }

        private void btnFechar_Click(object sender, EventArgs e)
        {
            timer2.Start();
        }

        

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (this.Opacity > 0.0)
            {
                this.Opacity -= 0.1;
            }
            else
            {
                timer2.Stop();
                Application.Exit();
            }
        }

        private void btnTimer_Click(object sender, EventArgs e)
        {
            rotinaPrincipal();
        }

        private void btnHabilitarTimer_Click(object sender, EventArgs e)
        {
            if (_TimerHabilitado) _TimerHabilitado = false;
            else _TimerHabilitado = true;

            mostrarHabilitarTimer();
            
        }

        public void mostrarHabilitarTimer()
        {
            if (!_TimerHabilitado)
            {
                _TimerHabilitado = false;
                botaoNaoDestacarconfiguracao(btnHabilitarTimer);
                btnHabilitarTimer.Text = "Habilitar Timer";

            }
            else
            {
                _TimerHabilitado = true;
                botaoDestacarConfiguracaoHabilitada(btnHabilitarTimer);
                btnHabilitarTimer.Text = "Timer Habilitado";
            }
        }

        private void btnDesabilitarTimer_Click(object sender, EventArgs e)
        {
            
        }

        private void btnAutoJogo_Click(object sender, EventArgs e)
        {

            if (_autojogo)
            {
                _autojogo = false;
                botaoNaoDestacarconfiguracao(btnAutoJogo);
                algoritmoNaoDestacar();
                btnAutoJogo.Text = "Ativar Jogadas Automaticas";
            }
            else
            {
                botaoDestacarConfiguracaoHabilitada(btnAutoJogo);
                algoritmoMostrarDestacar(-1,-1,-1);
                _autojogo = true;
                btnAutoJogo.Text = "Jogadas Automáticas";
            }

        }

        // Mover o Formulário pela tela
        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {  // Trap WM_NCHITTEST
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;  // HTCAPTION
                    return;
                }
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void btnConfiguracoes_Click(object sender, EventArgs e)
        {
            
            if (!_ConfiguracoesHabilitado)
            {
                this.Size = new System.Drawing.Size(880, 700);
                pbLogo.Location = new Point(284, 15);
                _ConfiguracoesHabilitado = true;
                botaoDestacarConfiguracaoHabilitada(btnConfiguracoes);
            }

            else
            {
                _ConfiguracoesHabilitado = false;
                botaoNaoDestacarconfiguracao(btnConfiguracoes);
                this.Size = new System.Drawing.Size(480, 700);
                pbLogo.Location = new Point(84, 15);
            }
        }

        private void btnSairPartida_Click(object sender, EventArgs e)
        {
            if(analisarStatus()!=(int)STATUS.PARTIDAENCERRADA)
            {
                DialogResult result = MessageBox.Show("Deseja sair da partida?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes) partidaSair();
                return;
            }

            partidaSair();
        }

        private void btnFechar_MouseHover(object sender, EventArgs e)
        {
            botaoDestacarPersonagemEliminado(btnFechar);
        }

        private void btnFechar_MouseLeave(object sender, EventArgs e)
        {
            botaoNaoDestacarPersonagens(btnFechar);
        }

        private void btnAlg10_Click(object sender, EventArgs e)
        {
            _algoritmoPromocao = (int)ALGORITMO.promocao10IniciarMeuReinado;
            algoritmoMostrarDestacar(-1, -1, -1);
        }

        private void btnAlg13_Click(object sender, EventArgs e)
        {
            _algoritmoPromocao = (int)ALGORITMO.promocao13OutrosdeMaiorNivel;
            algoritmoMostrarDestacar(-1, -1, -1);
        }

        private void btninfo_Click(object sender, EventArgs e)
        {
            if (_mostrarPaineldeInformacoes) _mostrarPaineldeInformacoes = false;
            else _mostrarPaineldeInformacoes = true;
            mostarOcultarInformacoes();
        }

        public void mostarOcultarInformacoes()
        {
            if (_mostrarPaineldeInformacoes == false)//ocultar painel de informacoes
            {
                lblEntrarPartida.Visible = true;
                lblInformacaoDesenvolvido.Visible = false;
                _mostrarPaineldeInformacoes = false;
                lblNomeJogador.Visible = true;
                lblSenhaPartida_Entrar.Visible = true;
                txtNomedoJogador.Visible = true;
                txtSenhadaPartidaEntrar.Visible = true;
                btnEntrarPartida.Visible = true;
                btninfo.Text = "?";
            }
            else
            { //ocultar painel de entrarPartida
                lblEntrarPartida.Visible = false;
                lblInformacaoDesenvolvido.Visible = true;
                _mostrarPaineldeInformacoes = true;
                lblNomeJogador.Visible = false;
                lblSenhaPartida_Entrar.Visible = false;
                txtNomedoJogador.Visible = false;
                txtSenhadaPartidaEntrar.Visible = false;
                btnEntrarPartida.Visible = false;
                btninfo.Text = "x";
            }
        }

        private void lstPartidas_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnHabilitarDebug_Click(object sender, EventArgs e)
        {
            if (_ativarDebug) _ativarDebug = false;
            else _ativarDebug = true;
            mostrarOcultarDebug();
        }

        public void mostrarOcultarDebug()
        {
            if (_ativarDebug)
            {
                btnHabilitarDebug.Text = "Debug Ativado";
                botaoDestacarConfiguracaoHabilitada(btnHabilitarDebug);
            }
            else
            {
                botaoNaoDestacarconfiguracao(btnHabilitarDebug);
                btnHabilitarDebug.Text = "Ativar Debug";
            }
        }

        private void btnEntrarPartidaEmergencia_Click(object sender, EventArgs e)
        {
            try
            {
                _myId = Convert.ToInt32(txtIdJogadorEmergencia.Text);
                _mySenha = txtSenhaJogadorEmergencia.Text;
                _partidaId = Convert.ToInt32(txtIdPartidaEmergencia.Text);
                _partidaSenha = txtSenhaPartidaEmergencia.Text;
                rotinaPartidaEntrar();
            }catch(Exception error)
            {
                MessageBox.Show(error.Message);
            }
            
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)numericUpDown1.Value * 1000;
        }

        private void btnSomenteAtualizarTabuleiro_Click(object sender, EventArgs e)
        {
            bool autojogosave = _autojogo;
            _autojogo = false;
            rotinaPrincipal();
            _autojogo = autojogosave;
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnMinimizar_MouseHover(object sender, EventArgs e)
        {
            botaoDestacarPersonagemEliminado(btnMinimizar);
        }

        private void btnMinimizar_MouseLeave(object sender, EventArgs e)
        {
            botaoNaoDestacarPersonagens(btnMinimizar);
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}