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
            posicionamentoPadrao, promocaoPadrao, VotacaoPadrao
                        , PosicionamentoEquilibrado, algoritmoPromocaoMeudeMenorNivel, VotacaoTenhoPontosSuficientes
                        , algoritmoPromocaoQualquerdeMenorNivel

        }

        //PROPRIEDADES
        public int _myId;
        public string _mySenha;
        public int _partidaId;
        public string _partidaSenha;
        //FLAGS DE CONTROLE
        public int _partidaStatus;
        public bool _rotinaPartidaIniciada;
        public bool _autojogo;
        public bool _TimerHabilitado;
        public int _nrodadaAtual;
        public bool _primeirajogadarealizada;
        public int _candidato;
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

        //PERSONAGENS
        public int _nPersonagens;
        public int[] _personagemStatus = new int[13];
        //-2 eliminado
        //-1 nao posicionado
        //x posicionado no novel x
        public bool[] _personagemMeu = new bool[13];
        public string[] _personagemCodinome = new string[13];

        //NIVEIS
        public int[] _nivelQtdPersonagens = new int[11];


        //VOTACAO

        //ARQUIVO DEBUG
        public StreamWriter debugfile;
        public string debugfilepath;
        public int debugfilecode;
        public int _counterDebug;
        public string[] _debuginfileRetornoServidor = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };

        //ACEBILIDADE
        public int _selected;


        //ALGORITMOS
        public int _algoritmoPromocao;
        public int _algoritmoPosicionamento;
        public int _algoritmoVotacao;
        public int[,] _jogadoresCartas = new int[,] { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
        public int[,] _contadorPromocaoJogadorPersonagem = new int[,] { { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };

        public frmPrincipal()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            inicio();
        }

        public void controlesVezdeOutro(int idoutrojogador)
        {
            btnPosicionar.Enabled = false;
            btnNao.Enabled = false;
            btnSim.Enabled = false;
            btnPromover.Enabled = false;
        }

        public void controlesMinhaVez(int status)
        {
            switch (status)
            {
                case (int)STATUS.POSICIONAR:
                    btnPosicionar.Enabled = true;
                    btnNao.Enabled = false;
                    btnSim.Enabled = false;
                    btnPromover.Enabled = false;
                    break;
                case (int)STATUS.PROMOVER:
                    btnPosicionar.Enabled = false;
                    btnNao.Enabled = false;
                    btnSim.Enabled = false;
                    btnPromover.Enabled = true;
                    break;
                case (int)STATUS.VOTAR:
                    btnPosicionar.Enabled = true;
                    if (_jogadoresCartasNaoDiponiveis[_meuCodigoJogador] > 0)
                    {
                        btnNao.Enabled = true;
                    }
                    else
                    {
                        btnNao.Enabled = false;
                    }
                    btnSim.Enabled = true;
                    btnPromover.Enabled = false;
                    break;
            }
        }




        public void inicio()
        {
            _counterDebug = 0;//utilizado no debug
            debugfilepath = GetTimestamp(DateTime.Now);
            debugfilecode = 0;

            lblVersao.Text = "MePresidentaServidor.dll | Versão " + Jogo.versao;
            lblDebugFileName.Text += debugfilepath;

            lblArquivoDebug.Text = "Arquivo Debug: " + lblDebugFileName.Text + ".txt";

            listarPartidas();
            inicializarVariaveisGlobais();
            inicializarJogadoresCartas();

            _selected = -1;

            //padronizar criacao partida para testes
            txtNomedaPartida.Text = "Londres";
            txtSenhaPartida.Text = "123";
            paineis(-1);
            timer1.Enabled = false;
            debugVariaveisGlobais("FormLoad");

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
            lblJogadas0.Visible = false;
            lblJogadas1.Visible = false;
            lblJogadas2.Visible = false;
            lblJogadas3.Visible = false;
            lblJogadas4.Visible = false;
            lblJogadas5.Visible = false;

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


        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
        public void novaRodada()
        {
            debug("novaRodada");
            _nPersonagens = listarPersonagens();
            inicializarNiveis();
            inicializarCartasNao();
            inicializarJogadoresCartas();
            listarCartas();
            tabuleiroLimpar();
        }

        public void paineis(int status)
        {
            switch (status)
            {
                case -1:
                    painelTabuleiro.Visible = false;
                    painelLobby.Visible = true;
                    break;
                case (int)STATUS.PARTIDAABERTA:
                    painelLobby.Visible = false;
                    btnIniciarPartida.Enabled = true;
                    painelTabuleiro.Visible = true;
                    btnAutoJogo.Text = "Configurações";
                    /*gbVotacao.Visible = false;
                    gbPosicionamento.Visible = false;
                    gbMinhasCartas.Visible = false;*/
                    break;
                case (int)STATUS.PARTIDAINICIADA:
                    painelLobby.Visible = false;
                    btnIniciarPartida.Enabled = false;
                    painelTabuleiro.Visible = true;
                    btnAutoJogo.Text = "Ativar Jogadas Automáticas"; 
                    break;
            }
        }




        public string servidor(int metodo)
        {
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
                    if (texto.Equals(_debuginfileRetornoServidor[metodo]))
                    {
                        debuginfile("RETORNO SERVIDOR " + metodoName + ": No Modifications", "RetornoServidor");
                    }
                    else
                    {
                        debuginfile("RETORNO SERVIDOR " + metodoName + ": " + texto, "RetornoServidor");
                        _debuginfileRetornoServidor[metodo] = texto;
                    }
                    if (!validarResultadoServidor(texto, metodo)) return null;
                    if (texto == "") return null;

                    return texto;
                }
                catch (Exception e)
                {
                    MessageBox.Show("ERRO DE SERVIDOR:" + e.Message);
                    continue;
                }
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!_TimerHabilitado)
            {
                btnHabilitarTimer.Enabled = true;
                btnDesabilitarTimer.Enabled = false;
                return;
            }
            else
            {
                btnDesabilitarTimer.Enabled = true;
                btnHabilitarTimer.Enabled = false;
            }
            timer1.Enabled = false;

            rotinaPrincipal();

            timer1.Enabled = true;
        }

        private bool validarResultadoServidor(string result, int metodo)
        {
            lblErro.Text = "Sucesso...";
            if (result.Length < 4) return true;
            if (result.Substring(0, 4) == "ERRO")
            {
                lblErro.Text = result;
                return false;
            }
            return true;
        }
        public bool verificarVez()
        {
            debug("verificarVez");

            string result = servidor(1);
            if (result == null)
            {
                debug("ERROR: verificarVez=false");
                return false;
            }

            //Analizar jogador da vez
            int jogadordaVez;
            string texto = result.Substring(0, result.Length - 2);
            int posicaoPrimeiraQuebra = result.IndexOf('\r');
            if (!Int32.TryParse(result.Substring(0, posicaoPrimeiraQuebra), out jogadordaVez))
            {
                debug("ERROR  -verificar vez= JOGADOR INDEFINIDO");
                return false;
            }
            debug("jogadordaVez = " + jogadordaVez + " - " + _jogadoresNome[getJogadorIndex(jogadordaVez)]);

            //atualizar tabuleiro
            if (texto.Length > posicaoPrimeiraQuebra)
            {
                _primeirajogadarealizada = true;
                tabuleiroAtualizar(texto.Substring(posicaoPrimeiraQuebra + 2));
                analisarUltimasJogadas();
                _pontuacaoAtual = calculoMinhaPontuacaoAtual();
            }
            else
            {
                if (_primeirajogadarealizada)
                {
                    lblNumRodada.Text = Convert.ToString(++_nrodadaAtual + 1) + "/3";
                    _primeirajogadarealizada = false;
                    novaRodada();
                }
                _pontuacaoTotal += _pontuacaoAtual;
                _pontuacaoAtual = 0;
                listarJogadores();//para atualizar pontuacao
                jogadoresMostrar();
            }


            lblPontuacaoAtual.Text = Convert.ToString(_pontuacaoAtual);
            lblPontuacaoTotal.Text = Convert.ToString(_pontuacaoTotal);


            int candidato = getCandidato();

            if ((_candidato > -1) && (candidato < 0))//nao e hora de votar mas houve votacao
            {
                analisarUltimaVotacao(_candidato);
                _candidato = -1;
            }



            jogadoresDestacar(jogadordaVez, _meuCodigoJogador);
            tabuleiroPosicionarDesempregados();

            if (_myId == jogadordaVez)
            {
                debug("MINHA VEZ=TRUE");
                return true;
            }

            //controlesVezdeOutro(jogadordaVez);
            debug("MINHA VEZ=FALSE");
            return false;
        }

        public void mostrarUltimaJogadaJogador(int jogador, string jogada)
        {
            switch (jogador)
            {
                case 0:
                    lblJogadas0.Text = jogada + " " + lblJogadas0.Text;
                    break;
                case 1:
                    lblJogadas1.Text = jogada + " " + lblJogadas1.Text;
                    break;
                case 2:
                    lblJogadas2.Text = jogada + " " + lblJogadas2.Text;
                    break;
                case 3:
                    lblJogadas3.Text = jogada + " " + lblJogadas3.Text;
                    break;
                case 4:
                    lblJogadas4.Text = jogada + " " + lblJogadas4.Text;
                    break;
                case 5:
                    lblJogadas5.Text = jogada + " " + lblJogadas5.Text;
                    break;

            }
        }

        public void algoritmoMostrarDestacar()
        {
            algoritmoNaoDestacar();
            switch (_algoritmoPosicionamento)
            {
                case (int)ALGORITMO.posicionamentoPadrao:
                    botaoDestacar1(btnAlgoritmoPosicionamentoPadrao);
                    botaoNaoDestacar(btnAlgoritmoPosicionamentoEquilibrado);
                    break;
                case (int)ALGORITMO.PosicionamentoEquilibrado:
                    botaoNaoDestacar(btnAlgoritmoPosicionamentoPadrao);
                    botaoDestacar1(btnAlgoritmoPosicionamentoEquilibrado);
                    break;
            }
            switch (_algoritmoPromocao)
            {
                case (int)ALGORITMO.promocaoPadrao:
                    botaoDestacar1(btnAlgoritmoPromocaoPadrao);
                    break;
                case (int)ALGORITMO.algoritmoPromocaoMeudeMenorNivel:
                    botaoDestacar1(btnAlgoritmoPromocaoMeudeMenorNivel);
                    break;
                case (int)ALGORITMO.algoritmoPromocaoQualquerdeMenorNivel:
                    botaoDestacar1(btnAlgoritmoPromocaoQualquerdeMenorNivel);
                    break;
            }
            switch (_algoritmoVotacao)
            {
                case (int)ALGORITMO.VotacaoPadrao:
                    botaoDestacar1(btnAlgoritmoVotacaoPadrao);
                    break;
                case (int)ALGORITMO.VotacaoTenhoPontosSuficientes:
                    botaoDestacar1(btnAlgoritmoVotacaoTenhoPontosSuficientes);
                    break;
            }

        }
        public void algoritmoNaoDestacar()
        {
            botaoNaoDestacar(btnAlgoritmoPosicionamentoPadrao);
            botaoNaoDestacar(btnAlgoritmoPromocaoPadrao);
            botaoNaoDestacar(btnAlgoritmoVotacaoPadrao);
            botaoNaoDestacar(btnAlgoritmoPosicionamentoEquilibrado);
            botaoNaoDestacar(btnAlgoritmoVotacaoTenhoPontosSuficientes);
            botaoNaoDestacar(btnAlgoritmoPromocaoMeudeMenorNivel);
            botaoNaoDestacar(btnAlgoritmoPromocaoQualquerdeMenorNivel);
        }


        public int analisarStatus()
        {
            debug("analisarStatus()");
            string status = servidor(14);
            if (status == null)
            {
                debug("analisarStatus()=Aberta");
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
            debug("ERRO: analisarStatus = INDEFINIDO");

            return -1;
        }
        private void analisarUltimaVotacao(int candidato)
        {
            debug("analisarUltimaVotacao");
            bool houveVotoNao = false;

            string result = servidor(13);
            if (result == null) return;



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
                mostrarUltimaJogadaJogador(i, Convert.ToString(++_jogadoresJogadacontador[i]) + "." + _personagemCodinome[candidato] + escolhavoto);
                if (escolhavoto.Equals("N"))
                {
                    //se jogador votou nao carta com certeza nao lhe pertence
                    houveVotoNao = true;
                    _jogadoresCartasNaoDiponiveis[i]--;
                    _jogadoresCartas[i, candidato] = 0;//Personagem com certeza não pertece ao jogador
                }
                else
                {
                    //se numeros de cartaz nao do jogador é 0, ha poucos nao no jogo e foi ele quem promoveu a carta era dele
                    if ((calculoQtdTotalAtualCartasNaoNoJogo() < (_nCartasNaoMax / 2)) && (_jogadoresCartasNaoDiponiveis[i] == 0) && _jogadoresUltimaJogadaAnalisada[i].Equals("J" + _personagemCodinome[candidato] + "10"))
                    {
                        _jogadoresCartas[i, candidato] += 2;
                    }

                    //se há um numero consideravel de cartas nao e nao ha muitos personagens eliminados a carta nao é dele
                    if ((calculoQtdTotalAtualCartasNaoNoJogo() > (_nCartasNaoMax / 2) && calculoNumeroPersonagensEliminados() < 5))
                    {
                        _jogadoresCartas[i, candidato] = 0;
                    }




                    _jogadoresCartas[i, candidato]++;
                }
                i++;
            }

            if (houveVotoNao)
            {
                _personagemStatus[candidato] = -2;//eliminado
                _nivelQtdPersonagens[10] = 0;
                eliminarCartaJogadoresCarta(candidato);
            }

        }
        public void analisarUltimasJogadas()
        {
            string result = servidor(15);
            if (result == null) return;

            debug("ULTIMA JOGADA = " + result);


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




                if (jogadorindex > -1)
                {
                    //if (jogadorindex == _meuCodigoJogador) continue;
                    if (_jogadoresUltimaJogadaAnalisada[jogadorindex].Equals(tipojogada + personagem + setor)) continue;
                    _jogadoresJogadacontador[jogadorindex]++;
                    if (tipojogada.Equals("S"))//Posicionamerto
                    {
                        if (setor.Equals("1"))//posicionamento no novel 1 supoe-se que personagem nao e do jogador
                        {
                            _jogadoresCartas[jogadorindex, personagemindex] = 0;
                        }
                    }
                    else if (tipojogada.Equals("J"))//PROMOCAO
                    {
                        _contadorPromocaoJogadorPersonagem[jogadorindex, personagemindex]++;
                        if (_contadorPromocaoJogadorPersonagem[jogadorindex, personagemindex] > 2)
                        {
                            _jogadoresCartas[jogadorindex, personagemindex]++;
                        }
                        if (setor.Equals("1"))//promocao para o nivel 1 supoe-se que personagem é do jogador
                        {
                            _jogadoresCartas[jogadorindex, personagemindex]++;
                        }

                    }

                    _jogadoresUltimaJogadaAnalisada[jogadorindex] = tipojogada + personagem + setor;
                    mostrarUltimaJogadaJogador(jogadorindex, Convert.ToString(_jogadoresJogadacontador[jogadorindex]) + "." + tipojogada + personagem + setor);
                }


            }
        }

        public void botaoDestacar1(System.Windows.Forms.Button b)
        {
            b.BackColor = Color.DeepSkyBlue;
            b.ForeColor = Color.FromArgb(9, 18, 23);
        }
        public void botaoDestacar2(System.Windows.Forms.Button b)
        {
            b.BackColor = Color.Black;
            b.ForeColor = Color.White;
        }

        public void botaoNaoDestacar(System.Windows.Forms.Button b)
        {
            b.BackColor = Color.FromArgb(9, 18, 23);
            b.ForeColor = Color.DeepSkyBlue;
        }

        public void debug(string texto)
        {
            //debugfile.Close();

            txtdebug.Text = "\r\n" + Convert.ToString(++_counterDebug) + "-" + texto + txtdebug.Text;

            debuginfile(texto, "");
        }

        public void debug(string texto, string nomedoarquivoadicional)
        {
            //debugfile.Close();

            ++_counterDebug;
            // txtdebug.Text = "\r\n" + Convert.ToString(++_counterDebug) + "-" + texto + txtdebug.Text;

            debuginfile(texto, nomedoarquivoadicional);

        }



        public void debuginfile(string texto, string nomeadicionaldoarquivo)
        {

            try
            {
                debugfile = File.AppendText(nomeadicionaldoarquivo + debugfilepath + Convert.ToString(debugfilecode) + ".txt");
            }
            catch (Exception e)//
            {
                //Caso nao consiga acessar o arquivo cria um novo arquivo debug
                //MessageBox.Show(e.Message);
                debugfile = File.AppendText(nomeadicionaldoarquivo + debugfilepath + Convert.ToString(++debugfilecode) + ".txt");
            }
            debugfile.WriteLine(Convert.ToString(++_counterDebug) + "-" + texto);
            debugfile.Close();
        }

        public void debugVariaveisGlobais(string rotulo)
        {
            string texto = "\r\n\r\n************PrintVariaveisGlobais[" + _counterDebug + "] - " + rotulo + "****************************\r\n";
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

            debuginfile(texto, "VariaveisGlobais");

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
            debug("ERRO: indice do jogador nao encontrado: " + nome);
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
            debug("ERRO: indice do jogador nao encontrado: " + id);
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
            debug("ERRO: indice do Personagem nao encontrado: " + nome);
            return 0;
        }

        private void inicializarCartasNao()
        {
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


            _myId = 0;
            _mySenha = "";
            _partidaSenha = "";
            _partidaId = 0;

            _partidaStatus = 0;
            _rotinaPartidaIniciada = false;
            //_autojogo = false;
            _TimerHabilitado = false;
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

            _algoritmoPromocao = (int)ALGORITMO.promocaoPadrao;
            _algoritmoPosicionamento = (int)ALGORITMO.posicionamentoPadrao;
            _algoritmoVotacao = (int)ALGORITMO.VotacaoPadrao;

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

        private void jogadoresDestacar(int jogadordaVez, int meucodigoJogador)
        {
            int i = 0;
            while (i < _nJogadores)
            {
                switch (i)
                {
                    case 0:

                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacar2(j0); }
                        else if (meucodigoJogador == i) { botaoDestacar1(j0); }
                        else { botaoNaoDestacar(j0); }
                        break;
                    case 1:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacar2(j1); }
                        else if (meucodigoJogador == i) { botaoDestacar1(j1); }
                        else { botaoNaoDestacar(j1); }
                        break;
                    case 2:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacar2(j2); }
                        else if (meucodigoJogador == i) { botaoDestacar1(j2); }
                        else { botaoNaoDestacar(j2); }
                        break;
                    case 3:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacar2(j3); }
                        else if (meucodigoJogador == i) { botaoDestacar1(j3); }
                        else { botaoNaoDestacar(j3); }
                        break;
                    case 4:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacar2(j4); }
                        else if (meucodigoJogador == i) { botaoDestacar1(j4); }
                        else { botaoNaoDestacar(j4); }
                        break;
                    case 5:
                        if ((jogadordaVez == _jogadoresId[i] && meucodigoJogador == i) || (jogadordaVez == _jogadoresId[i])) { botaoDestacar2(j5); }
                        else if (meucodigoJogador == i) { botaoDestacar1(j5); }
                        else { botaoNaoDestacar(j5); }
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

            debug("NUMERO DA PARTIDA CRIADA = " + x);
            txtNomedoJogador.Focus();
            listarPartidas();

        }
        private void partidaEntrar(int idPartida, string senhaPartida)
        {
            string result = servidor(7);

            if (result == null) return;


            string[] campos = result.Split(',');

            debug("ENTRAR PARTIDA = " + campos[0] + "  " + campos[1]);

            //atualizar propriedades
            _partidaId = idPartida;
            _partidaSenha = senhaPartida;
            _myId = Convert.ToInt32(campos[0]);
            _mySenha = campos[1];

            _nJogadores = listarJogadores();
            jogadoresMostrar();
            timer1.Enabled = true;
            _TimerHabilitado = true;
            rotinaPrincipal();
        }

        public void partidaIniciar()
        {
            string result = servidor(9);

            if (result == null) return;

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
            debug("POSICIONAR PERSONAGEM OK");
            return true;
        }

        public bool personagemPromover()
        {
            string result = servidor(10);
            if (result == null) return false;
            debug("PROMOCAO PERSONAGEM OK");
            return true;

        }

        public bool personagemVotar(int escolha)
        {

            int candidato = getCandidato();

            string result;
            if (escolha > 0)
            {
                debug("MEU VOTO SIM");
                result = servidor(11);
            }
            else
            {
                debug("MEU VOTO NAO");
                result = servidor(12);
            }

            if (result == null) return false;

            _candidato = candidato;

            debug("VOTACAO OK");
            return true;

        }

        public void rotinaPrincipal()
        {
            int status = analisarStatus();
            if (status < 0) return;

            switch (status)
            {
                case (int)STATUS.PARTIDAABERTA:

                    rotinaPartidaAberta();
                    break;

                case (int)STATUS.POSICIONAR:
                    if (!_rotinaPartidaIniciada) rotinaPartidaIniciada();
                    if (verificarVez())
                    {
                        // controlesMinhaVez(status);
                        if (_autojogo)
                        {
                            if (!autoposicionar(_algoritmoPosicionamento))
                            {
                                debug("ERRO AO AUTOPOSICIONAR");
                            }
                        }
                    }
                    break;
                case (int)STATUS.PROMOVER:
                    if (!_rotinaPartidaIniciada) rotinaPartidaIniciada();
                    if (verificarVez())
                    {
                        //controlesMinhaVez(status);
                        if (_autojogo)
                        {
                            if (!autopromover(_algoritmoPromocao))
                            {
                                debug("ERRO AO AUTOPROMOVER");
                            }
                        }
                    }

                    break;
                case (int)STATUS.VOTAR:
                    if (!_rotinaPartidaIniciada) rotinaPartidaIniciada();
                    if (verificarVez())
                    {
                        //controlesMinhaVez(status);
                        if (_autojogo)
                        {
                            if (!autovotar(_algoritmoVotacao))
                            {
                                debug("ERRO AO AUTOVOTAR");
                            }
                        }
                    }
                    break;
                case (int)STATUS.PARTIDAENCERRADA:
                    rotinaPartidaEncerrada();
                    break;
                default:
                    debug("ERRO: rotina = Status Indefinido: " + status);
                    break;
            }
            debugVariaveisGlobais("Rotina");
        }

        public void rotinaPartidaAberta()
        {
            _nJogadores = listarJogadores();
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
            lblPontuacaoTotal.BackColor = Color.Yellow;
            lblErro.Text = "Partida Encerrada!!";
            timer1.Enabled = false;

            listarJogadores();
            jogadoresMostrar();
            inicializarCartasNao();
            jogadoresDestacar(-1, _meuCodigoJogador);
            paineis((int)STATUS.PARTIDAABERTA);
        }
        public void rotinaPartidaIniciada()
        {
            _nJogadores = listarJogadores();
            jogadoresMostrar();
            novaRodada();
            listarSetores();
            btnIniciarPartida.Enabled = false;
            _rotinaPartidaIniciada = true;//evita que esta funcao seja chamada mais de uma vez
            paineis((int)STATUS.PARTIDAINICIADA);
        }

        public void tabuleiroLimpar()
        {
            n01.Text = "";
            n01.BackColor = Color.FromArgb(9, 18, 23);
            n02.Text = "";
            n02.BackColor = Color.FromArgb(9, 18, 23);
            n03.Text = "";
            n03.BackColor = Color.FromArgb(9, 18, 23);
            n04.Text = "";
            n04.BackColor = Color.FromArgb(9, 18, 23);

            n11.Text = "";
            n11.BackColor = Color.FromArgb(9, 18, 23);
            n12.Text = "";
            n12.BackColor = Color.FromArgb(9, 18, 23);
            n13.Text = "";
            n13.BackColor = Color.FromArgb(9, 18, 23);
            n14.Text = "";
            n14.BackColor = Color.FromArgb(9, 18, 23);

            n21.Text = "";
            n21.BackColor = Color.FromArgb(9, 18, 23);
            n22.Text = "";
            n22.BackColor = Color.FromArgb(9, 18, 23);
            n23.Text = "";
            n23.BackColor = Color.FromArgb(9, 18, 23);
            n24.Text = "";
            n24.BackColor = Color.FromArgb(9, 18, 23);

            n31.Text = "";
            n31.BackColor = Color.FromArgb(9, 18, 23);
            n32.Text = "";
            n32.BackColor = Color.FromArgb(9, 18, 23);
            n33.Text = "";
            n33.BackColor = Color.FromArgb(9, 18, 23);
            n34.Text = "";
            n34.BackColor = Color.FromArgb(9, 18, 23);

            n41.Text = "";
            n41.BackColor = Color.FromArgb(9, 18, 23);
            n42.Text = "";
            n42.BackColor = Color.FromArgb(9, 18, 23);
            n43.Text = "";
            n43.BackColor = Color.FromArgb(9, 18, 23);
            n44.Text = "";
            n44.BackColor = Color.FromArgb(9, 18, 23);

            n51.Text = "";
            n51.BackColor = Color.FromArgb(9, 18, 23);
            n52.Text = "";
            n52.BackColor = Color.FromArgb(9, 18, 23);
            n53.Text = "";
            n53.BackColor = Color.FromArgb(9, 18, 23);
            n54.Text = "";
            n54.BackColor = Color.FromArgb(9, 18, 23);

            n10.Text = "";
            n10.BackColor = Color.FromArgb(9, 18, 23);
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
                    _personagemStatus[i] = -1;
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
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd0);
                            btnd0.Text = _personagemCodinome[personagem];
                        }
                        else
                        {

                            btnd0.Text = Convert.ToString(status);//* teste - apresentar nivel que meu personagem esta
                            if (_personagemMeu[personagem]) btnd0.Text = Convert.ToString(status);
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd0);
                        }
                        break;
                    case 1:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd1);
                            btnd1.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd1.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd1);
                        }
                        break;
                    case 2:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd2);
                            btnd2.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd2.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd2);
                        }

                        break;
                    case 3:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd3);
                            btnd3.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd3.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd3);
                        }

                        break;
                    case 4:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd4);
                            btnd4.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd4.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd4);
                        }

                        break;
                    case 5:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd5);
                            btnd5.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd5.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd5);
                        }

                        break;
                    case 6:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd6);
                            btnd6.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd6.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd6);
                        }

                        break;
                    case 7:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd7);
                            btnd7.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd7.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd7);
                        }

                        break;
                    case 8:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd8);
                            btnd8.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd8.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd8);
                        }

                        break;
                    case 9:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd9);
                            btnd9.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd9.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd9);
                        }

                        break;
                    case 10:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd10);
                            btnd10.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd10.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd10);
                        }

                        break;
                    case 11:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd11);
                            btnd11.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd11.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd11);
                        }

                        break;
                    case 12:
                        if (status < 0)
                        {
                            if (_personagemMeu[personagem]) botaoDestacar1(btnd12);
                            btnd12.Text = _personagemCodinome[personagem];
                        }
                        else
                        {
                            btnd12.Text = "";
                        }
                        if (!_personagemMeu[personagem])
                        {
                            botaoNaoDestacar(btnd12);
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
                    debug("ERRO FUNCAO: posicionarPersonagem(): cdPersonagem nao encontrado");
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
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n01);
                    }
                    else if (n02.Text == "")
                    {
                        n02.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n02);
                    }
                    else if (n03.Text == "")
                    {
                        n03.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n03);
                    }
                    else if (n04.Text == "")
                    {
                        n04.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n04);
                    }
                    break;
                case 1:
                    if (n11.Text == "")
                    {
                        n11.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n11);
                    }
                    else if (n12.Text == "")
                    {
                        n12.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n12);
                    }
                    else if (n13.Text == "")
                    {
                        n13.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n13);
                    }
                    else if (n14.Text == "")
                    {
                        n14.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n14);
                    }
                    break;
                case 2:
                    if (n21.Text == "")
                    {
                        n21.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n21);
                    }
                    else if (n22.Text == "")
                    {
                        n22.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n22);
                    }
                    else if (n23.Text == "")
                    {
                        n23.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n23);
                    }
                    else if (n24.Text == "")
                    {
                        n24.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n24);
                    }

                    break;
                case 3:
                    if (n31.Text == "")
                    {
                        n31.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n31);
                    }
                    else if (n32.Text == "")
                    {
                        n32.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n32);
                    }
                    else if (n33.Text == "")
                    {
                        n33.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n33);
                    }
                    else if (n34.Text == "")
                    {
                        n34.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n34);
                    }

                    break;
                case 4:
                    if (n41.Text == "")
                    {
                        n41.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n41);
                    }
                    else if (n42.Text == "")
                    {
                        n42.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n42);
                    }
                    else if (n43.Text == "")
                    {
                        n43.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n43);
                    }
                    else if (n44.Text == "")
                    {
                        n44.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n44);
                    }

                    break;
                case 5:
                    if (n51.Text == "")
                    {
                        n51.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n51);
                    }
                    else if (n52.Text == "")
                    {
                        n52.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n52);
                    }
                    else if (n53.Text == "")
                    {
                        n53.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n53);
                    }
                    else if (n54.Text == "")
                    {
                        n54.Text = personagem;
                        if (_personagemMeu[cdPersonagem]) botaoDestacar1(n54);
                    }

                    break;
                case 10:
                    n10.Text = personagem;
                    if (_personagemMeu[cdPersonagem]) botaoDestacar1(n10);
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
                        n01.BackColor = Color.FromArgb(9, 18, 23);

                    }
                    else if (n02.Text == personagem)
                    {
                        n02.Text = "";
                        n02.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n03.Text == personagem)
                    {
                        n03.Text = "";
                        n03.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n04.Text == personagem)
                    {
                        n04.Text = "";
                        n04.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    break;
                case 1:
                    if (n11.Text == personagem)
                    {
                        n11.Text = "";
                        n11.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n12.Text == personagem)
                    {
                        n12.Text = "";
                        n12.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n13.Text == personagem)
                    {
                        n13.Text = "";
                        n13.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n14.Text == personagem)
                    {
                        n14.Text = "";
                        n14.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    break;
                case 2:
                    if (n21.Text == personagem)
                    {
                        n21.Text = "";
                        n21.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n22.Text == personagem)
                    {
                        n22.Text = "";
                        n22.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n23.Text == personagem)
                    {
                        n23.Text = "";
                        n23.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n24.Text == personagem)
                    {
                        n24.Text = "";
                        n24.BackColor = Color.FromArgb(9, 18, 23);
                    }

                    break;
                case 3:
                    if (n31.Text == personagem)
                    {
                        n31.Text = "";
                        n31.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n32.Text == personagem)
                    {
                        n32.Text = "";
                        n32.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n33.Text == personagem)
                    {
                        n33.Text = "";
                        n33.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n34.Text == personagem)
                    {
                        n34.Text = "";
                        n34.BackColor = Color.FromArgb(9, 18, 23);
                    }

                    break;
                case 4:
                    if (n41.Text == personagem)
                    {
                        n41.Text = "";
                        n41.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n42.Text == personagem)
                    {
                        n42.Text = "";
                        n42.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n43.Text == personagem)
                    {
                        n43.Text = "";
                        n43.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n44.Text == personagem)
                    {
                        n44.Text = "";
                        n44.BackColor = Color.FromArgb(9, 18, 23);
                    }

                    break;
                case 5:
                    if (n51.Text == personagem)
                    {
                        n51.Text = "";
                        n51.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n52.Text == personagem)
                    {
                        n52.Text = "";
                        n52.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n53.Text == personagem)
                    {
                        n53.Text = "";
                        n53.BackColor = Color.FromArgb(9, 18, 23);
                    }
                    else if (n54.Text == personagem)
                    {
                        n54.Text = "";
                        n54.BackColor = Color.FromArgb(9, 18, 23);
                    }

                    break;
                case 10:
                    n10.Text = "";
                    n10.BackColor = Color.FromArgb(9, 18, 23);
                    break;
            }
        }

        //PARTE 2

        private void btnCriarPartida_Click(object sender, EventArgs e)
        {
            partidaCriar();
        }

        private void btnEntrarPartida_Click(object sender, EventArgs e)
        {
            int idpartida;
            if (Int32.TryParse(lblIdPartida.Text, out idpartida))
            {
                partidaEntrar(idpartida, txtSenhadaPartidaEntrar.Text);
            }
            else
            {
                debug("ERROR: NUMERO DE PARTIDA INVALIDA");
            }
        }

        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            listarPartidas();
        }

        private void frmPrincipal_Load(object sender, EventArgs e)
        {

        }

        private void btnAutoJogo_Click(object sender, EventArgs e)
        {
            
            if (_autojogo)
            {
                _autojogo = false;
                botaoNaoDestacar(btnAutoJogo);
                algoritmoNaoDestacar();
                btnAutoJogo.Text = "Ativar Jogadas Automaticas";
            }
            else
            {
                botaoDestacar1(btnAutoJogo);
                algoritmoMostrarDestacar();
                _autojogo = true;
                btnAutoJogo.Text = "Jogadas Automáticas";
            }
           
        }

        private void btnPosicionar_Click(object sender, EventArgs e)
        {
            int setor;
            if (!Int32.TryParse(txtSetor.Text, out setor))
            {
                MessageBox.Show("SETOR INVALIDO");
            }

            personagemPosicionar();
        }

        private void btnIniciarPartida_Click(object sender, EventArgs e)
        {
            partidaIniciar();
        }

        private void btnNao_Click(object sender, EventArgs e)
        {
            personagemVotar(0);
        }

        private void btnPromover_Click(object sender, EventArgs e)
        {
            personagemPromover();
        }

        private void btnSim_Click(object sender, EventArgs e)
        {
            personagemVotar(1);
        }

        private void btnAlgoritmoPosicionamentoEquilibrado_Click(object sender, EventArgs e)
        {
            _algoritmoPosicionamento = (int)ALGORITMO.PosicionamentoEquilibrado;
            algoritmoMostrarDestacar();
        }

        private void btnAlgoritmoPosicionamentoPadrao_Click(object sender, EventArgs e)
        {
            _algoritmoPosicionamento = (int)ALGORITMO.posicionamentoPadrao;
            algoritmoMostrarDestacar();
        }

        private void btnAlgoritmoPromocaoPadrao_Click(object sender, EventArgs e)
        {
            _algoritmoPromocao = (int)ALGORITMO.promocaoPadrao;
            algoritmoMostrarDestacar();
        }

        private void btnAlgoritmoVotacaoPadrao_Click(object sender, EventArgs e)
        {
            _algoritmoVotacao = (int)ALGORITMO.VotacaoPadrao;
            algoritmoMostrarDestacar();
        }

        private void btnAlgoritmoPromocaoMeudeMenorNivel_Click(object sender, EventArgs e)
        {
            _algoritmoPromocao = (int)ALGORITMO.algoritmoPromocaoMeudeMenorNivel;
            algoritmoMostrarDestacar();
        }

        private void btnAlgoritmoVotacaoTenhoPontosSuficientes_Click(object sender, EventArgs e)
        {
            _algoritmoVotacao = (int)ALGORITMO.VotacaoTenhoPontosSuficientes;
            algoritmoMostrarDestacar();
        }

        private void btnAlgoritmoPromocaoQualquerdeMenorNivel_Click(object sender, EventArgs e)
        {
            _algoritmoPromocao = (int)ALGORITMO.algoritmoPromocaoQualquerdeMenorNivel;
            algoritmoMostrarDestacar();
        }

        public bool autoposicionar(int algoritmo)
        {
            debug("autoposicionar");
            //posicionar personagens nos niveis mais altos independente de serem meus ou nao em ordem alfabetica
            if (getCandidato() > -1)
            {
                debug("ERRO ALGORITMO: chamou posicionamento ao inves de votacao");
                return false;
            }

            if (calculoNumeroPersonagensDesempregados() < 1)
            {
                debug("ERRO ALGORITMO: chamou posicionamento ao inves de promocao");
                return false;
            }


            switch (algoritmo)
            {
                case (int)ALGORITMO.posicionamentoPadrao:
                    if (algoritmoPosicionarPadrao())
                    {
                        return true;
                    }
                    else
                    {
                        debug("ERRO: algoritmo padrao nao funcionou");
                    }
                    break;
                case (int)ALGORITMO.PosicionamentoEquilibrado:
                    if (algoritmoPosicionamentoEquilibrado())
                    {
                        return true;
                    }
                    else
                    {
                        debug("ERRO: algoritmo posicionamento equilibrado nao funcionou");
                    }
                    return autoposicionar((int)ALGORITMO.posicionamentoPadrao);
            }
            return false;
        }




        public bool autopromover(int algoritmoPromocao)
        {
            debug("autopromover");
            //if (!esperar()) return;
            //promover personagens em ordem alfabetica independentende de serem meus ou nao
            if (getCandidato() > -1)
            {
                debug("ERRO ALGORITMO: chamou promocao ao inves de votacao");
                return false;
            }

            if (calculoNumeroPersonagensDesempregados() > 0)
            {
                debug("ERRO ALGORITMO: chamou promocao ao inves de posicionamento");
                return false;
            }







            switch (algoritmoPromocao)
            {
                case (int)ALGORITMO.promocaoPadrao:
                    if (!algoritmoPromocaoPadrao(-1))
                    {
                        debug("ERRO ALGORITMO: Promocao Padrao");
                    }
                    else
                    {
                        return true;
                    }
                    break;
                case (int)ALGORITMO.algoritmoPromocaoMeudeMenorNivel:


                    //se quantidade de cartas nao dos oponentes é 0
                    if (calculoQtdCartasNaoQueNaoSaoMinhas() <= 0)
                    {
                        if (algoritmoPromocaoPadrao(calculoPersonagemMeudeMaiorNivel()))
                        {
                            return true;
                        }
                        else
                        {
                            return autopromover((int)ALGORITMO.promocaoPadrao);
                        }
                    }

                    //simplesmente aumentar minha pontuacao
                    if (algoritmoPromocaoMeudeMenorNivel())
                    {
                        return true;
                    }
                    else
                    {
                        debug("ERRO algoritmoPromocaoMeudeMenorNivel");
                        if (algoritmoPromocaoOutrosdeMaiorNivel())
                        {
                            return true;
                        }
                        return autopromover((int)ALGORITMO.algoritmoPromocaoQualquerdeMenorNivel);
                    }

                case (int)ALGORITMO.algoritmoPromocaoQualquerdeMenorNivel:
                    if (!algoritmoPromocaoQualquerdeMenorNivel())
                    {
                        debug("ERRO algoritmoPromocaoQualquerdeMenorNivel");
                        return autopromover((int)ALGORITMO.promocaoPadrao);

                    }
                    else
                    {
                        return true;
                    }
            }


            return false;

        }
        public bool autovotar(int algoritmo)
        {
            debug("autovotar");
            //if (!esperar()) return;
            //votar sim para meus personagens e nao para outros
            if (getCandidato() < 0)
            {
                debug("ERRO ALGORITMO: Não ha candidato para autovotar");
                return false;
            }

            if (_jogadoresCartasNaoDiponiveis[_meuCodigoJogador] < 1)
            {
                return personagemVotar(1);
            }

            switch (algoritmo)
            {
                case (int)ALGORITMO.VotacaoPadrao:
                    if (algoritmoVotacaoPadrao()) return true;
                    else
                    {
                        debug("ERRO ALGORTMO - algortimovotacaopadrao retornou falso");
                    }
                    break;
                case (int)ALGORITMO.VotacaoTenhoPontosSuficientes:
                    if (algoritmoVotacaoTenhoPontosSuficientes()) return true;
                    else
                    {
                        debug("ERRO ALGORTMO - algortimovotacaoTenhospontossuficientes retornou falso");
                        return autovotar((int)ALGORITMO.VotacaoPadrao);
                    }

            }
            return false;
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
            debug("calculoNumeroPersonagensEliminados=" + soma);
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
            debug("calculoNumeroPersonagensDesempregados=" + soma);
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

            debug("calculoQtdCartasNaoQueNaoSaoMinhas=" + soma);
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
            debug("calculoQtdTotalAtualCartasNaoNoJogo=" + soma);
            return soma;
        }
        public int calculoPersonagemMeudeMaiorNivel()
        {
            debug("calculoPersonagemMeudeMaiorNivel");
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
                            debug("calculoPersonagemMeudeMaiorNivel=" + personagem + "(" + _personagemCodinome[personagem] + ")");
                            return personagem;
                        }
                    }
                    personagem++;
                }
                nivel--;
            }

            debug("calculoPersonagemdeMaiorNivel=INDEFINIDO");
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
            debug("calculoMinhaPontuacaoAtual");
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

            return pontos;
        }

        private int calculoMinhaPontuacaoTotalAtual()
        {
            int pontos = calculoMinhaPontuacaoAtual() + _jogadoresPontos[_meuCodigoJogador];
            debug("calculoMinhaPontuacaoTotalAtual=" + pontos);
            return pontos;
        }

        public int calculoMaiorPontuacaoPossivelAtual()
        {
            debug("calculoMaiorPontuacaoPossivelAtual");
            int nivel = 5;
            int personagem = 0;
            int[] soma = new int[6];
            int contador = 0;
            int jogador = 0;

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

                                if (contador == 5) break;
                                //somou os cinco maiores

                            }
                            personagem++;
                        }
                        if (contador == 5) break;

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

            return somaMaior + 10; //soma ao presidente
        }

        public int calculoQtdMeusPersonagensNoNivel(int nivel)
        {
            debug("calculoQtdMeusPersonagensNoNivel");
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
            return contador;
        }

        public bool algoritmoPosicionarPadrao()
        {
            debug("algoritmoPosicionarPadrao");
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

                            debug("algoritmoPosicionarPadrao - Tentar posicionar " + personagem + "(" + _personagemCodinome[personagem] + ")");

                            if (personagemPosicionar())
                            {
                                return true;
                            }
                            debug("algoritmoPosicionarPadrao - personagemPosicionar()=false");
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
            debug("algoritmoPosicionamentoEquilibrado");
            int personagem = 0;
            while (personagem < _personagemStatus.Length)
            {
                if (calculoPersonagemDesempregado(personagem))
                {
                    if (_personagemMeu[personagem])
                    {
                        int nivel = 4;
                        while (nivel > 0)
                        {
                            if (calculoQtdMeusPersonagensNoNivel(nivel) < 2 && calculoNivelDisponivel(nivel))
                            {
                                txtPersonagem.Text = _personagemCodinome[personagem];
                                txtSetor.Text = Convert.ToString(nivel);
                                if (personagemPosicionar()) return true;
                            }
                            nivel--;
                        }
                    }
                }
                personagem++;
            }
            return false;
        }


        public bool algoritmoPromocaoPadrao(int personagemPrioritario)
        {

            debug("algoritmoPromocaoPadrao");
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
            debug("algoritmoPromocaoMeudeMenorNivel");
            int personagem = 0;
            int nivel = 0;
            while (nivel < 5)
            {
                personagem = _personagemStatus.Length - 1;
                while (personagem >= 0)
                {
                    if (_personagemMeu[personagem] && _personagemStatus[personagem] > -1)
                    {
                        if ((_nivelQtdPersonagens[_personagemStatus[personagem] + 1]) < 4)
                        {
                            txtPersonagem.Text = _personagemCodinome[personagem];
                            if (personagemPromover()) return true;
                        }
                    }
                    personagem--;
                }
                nivel++;
            }
            return false;
        }
        public bool algoritmoPromocaoOutrosdeMaiorNivel()
        {
            debug("algoritmoPromocaoOutrosdeMaiorNivel");

            if (calculoQtdCartasNaoQueNaoSaoMinhas() < 4)
            {
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
                            if ((_nivelQtdPersonagens[_personagemStatus[personagem] + 1]) < 4)
                            {
                                txtPersonagem.Text = _personagemCodinome[personagem];
                                if (personagemPromover()) return true;
                            }
                        }
                    }
                    personagem++;
                }
                nivel--;
            }
            return false;
        }

        public bool algoritmoPromocaoQualquerdeMenorNivel()
        {
            debug("algoritmoPromocaoQualquerdeMenorNivel");
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
                            if (personagemPromover()) return true;
                        }
                    }
                    personagem++;
                }
                nivel++;
            }
            return false;
        }




        public bool algoritmoVotacaoPadrao()
        {
            debug("algoritmoVotacaoPadrao");
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

            debug("algoritmoVotacaoTenhoPontosSuficientes");
            int minhaPontuacaoTotalAtual = _jogadoresPontos[_meuCodigoJogador] + calculoMinhaPontuacaoAtual();


            //_TimerHabilitado = false;



            //Ninguem mais pode ganhar de mim
            if (calculoMaiorPontuacaoTotalAtualPossivel() < minhaPontuacaoTotalAtual)
            {
                return personagemVotar(1);
            }
            //ALguem pode conseguir mais pontos que eu



            //se for possivel mais pontos
            return false;
        }

        public int calculoMaiorPontuacaoTotalAtualPossivel()
        {
            debug("calculoMaiorPontuacaoTotalAtualPossivel");
            int i = 0;
            int maiorPontuacaoPossivelAtual = calculoMaiorPontuacaoPossivelAtual();
            int maiorPontuacaoTotalAtualPossivel = 0;
            while (i < _nJogadores)
            {
                if (i != _meuCodigoJogador)
                {
                    if ((_jogadoresPontos[i] + maiorPontuacaoPossivelAtual) > maiorPontuacaoTotalAtualPossivel)
                    {
                        maiorPontuacaoTotalAtualPossivel += _jogadoresPontos[i] + maiorPontuacaoPossivelAtual;
                    }
                    debug("calculoMaiorPontuacaoTotalAtualPossivel" + "/[" + i + "]" + _jogadoresPontos[i] + maiorPontuacaoPossivelAtual);
                }
                i++;
            }

            return maiorPontuacaoTotalAtualPossivel;
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
                lblNomeJogador.Visible = false;
                lblSenhaPartida_Entrar.Visible = false;
                txtNomedoJogador.Visible = false;
                txtSenhadaPartidaEntrar.Visible = false;
                btnEntrarPartida.Visible = false;
                lblEntrarPartida.Visible = false;
                lblInformacaoDesenvolvido.Visible = true;
                MessageBox.Show("Erro: A partida não está aberta!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                lblIdPartida.Text = lstPartidas.SelectedItems[0].SubItems[0].Text;
                lblNomeJogador.Visible = true;
                lblSenhaPartida_Entrar.Visible = true;
                txtNomedoJogador.Visible = true;
                txtSenhadaPartidaEntrar.Visible = true;
                btnEntrarPartida.Visible = true;
                lblEntrarPartida.Visible = true;
                lblInformacaoDesenvolvido.Visible = false;

                MessageBox.Show("A Partida " + lstPartidas.SelectedItems[0].SubItems[1].Text + " foi selecionada com sucesso!\n\nInsira o nome do jogador e a senha da \npartida no canto inferior direito para \nentrar na partida.", "Partida selecionada", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                this.Opacity -= 0.03;
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
            _TimerHabilitado = true;
            btnHabilitarTimer.Enabled = false;
            btnDesabilitarTimer.Enabled = true;
        }

        private void btnDesabilitarTimer_Click(object sender, EventArgs e)
        {
            _TimerHabilitado = false;
            btnDesabilitarTimer.Enabled = false;
            btnHabilitarTimer.Enabled = true;
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
            if (this.Size.Width == 480)
                this.Size = new System.Drawing.Size(691, 700);
            else
                this.Size = new System.Drawing.Size(480, 700);
        }
    }
}