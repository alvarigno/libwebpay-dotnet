﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Webpay.Transbank.Library;
using Webpay.Transbank.Library.Wsdl.Capture;
using Webpay.Transbank.Library.Wsdl.Normal;

/**
  * @brief      Ecommerce Plugin for chilean Webpay
  * @category   Plugins/SDK
  * @author     Allware Ltda. (http://www.allware.cl)
  * @copyright  2015 Transbank S.A. (http://www.tranbank.cl)
  * @date       Jan 2015
  * @license    GNU LGPL
  * @version    1.0
  * @link       http://transbankdevelopers.cl/
  *
  * This software was created for easy integration of ecommerce
  * portals with Transbank Webpay solution.
  *
  * Required:
  *  - .NET Framework 4.5
  *
  * Changelog:
  *  - v1.0 Original release
  *
  * See documentation and how to install at link site
  *
  */

namespace Transbank.NET
{
    public partial class tbk_capture : System.Web.UI.Page
    {

        /** Mensaje de Ejecución */
        private string message;

        /** Crea Dictionary con datos Integración Pruebas */
        private Dictionary<string, string> certificate = Transbank.NET.sample.certificates.CertCapture.certificate();

        /** Crea Dictionary con datos de entrada */
        private Dictionary<string, string> request = new Dictionary<string, string>();

        protected void Page_Load(object sender, EventArgs e)
        {

            Configuration configuration = new Configuration();
            configuration.Environment = certificate["environment"];
            configuration.CommerceCode = certificate["commerce_code"];
            configuration.PublicCert = certificate["public_cert"];
            configuration.WebpayCert = certificate["webpay_cert"];
            configuration.Password = certificate["password"];

            /** Creacion Objeto Webpay */
            Webpay.Transbank.Library.Webpay webpay = new Webpay.Transbank.Library.Webpay(configuration);

            /** Información de Host para crear URL */
            String httpHost = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_HOST"].ToString();
            String selfURL = System.Web.HttpContext.Current.Request.ServerVariables["URL"].ToString();

            string action = !String.IsNullOrEmpty(HttpContext.Current.Request.QueryString["action"]) ? HttpContext.Current.Request.QueryString["action"] : "init";

            /** Crea URL de Aplicación */
            string sample_baseurl = "http://" + httpHost + selfURL;

            /** Crea Dictionary con descripción */
            Dictionary<string, string> description = new Dictionary<string, string>();

            description.Add("VD", "Venta Deb&iacute;to");
            description.Add("VN", "Venta Normal");
            description.Add("VC", "Venta en cuotas");
            description.Add("SI", "cuotas sin inter&eacute;s");
            description.Add("S2", "2 cuotas sin inter&eacute;s");
            description.Add("NC", "N cuotas sin inter&eacute;s");

            /** Crea Dictionary con codigos de resultado */
            Dictionary<string, string> codes = new Dictionary<string, string>();

            codes.Add("0", "Transacci&oacute;n aprobada");
            codes.Add("-1", "Rechazo de transacci&oacute;n");
            codes.Add("-2", "Transacci&oacute;n debe reintentarse");
            codes.Add("-3", "Error en transacci&oacute;n");
            codes.Add("-4", "Rechazo de transacci&oacute;n");
            codes.Add("-5", "Rechazo por error de tasa");
            codes.Add("-6", "Excede cupo m&aacute;ximo mensual");
            codes.Add("-7", "Excede l&iacute;mite diario por transacci&oacute;n");
            codes.Add("-8", "Rubro no autorizado");

            HttpContext.Current.Response.Write("<p style='font-weight: bold; font-size: 200%;'>Ejemplos Webpay - Transacci&oacute;n Normal Captura Diferida</p>");

            string buyOrder;

            string tx_step = "";

            switch (action)
            {

                default:

                    tx_step = "Init";

                    try
                    {

                        HttpContext.Current.Response.Write("<p style='font-weight: bold; font-size: 150%;'>Step: " + tx_step + "</p>");

                        Random random = new Random();

                        /** Monto de la transacción */
                        decimal amount = System.Convert.ToDecimal("9990");

                        /** Orden de compra de la tienda */
                        buyOrder = random.Next(0, 1000).ToString();

                        /** (Opcional) Identificador de sesión, uso interno de comercio */
                        string sessionId = random.Next(0, 1000).ToString();

                        /** URL Final */
                        string urlReturn = sample_baseurl + "?action=result";

                        /** URL Final */
                        string urlFinal = sample_baseurl + "?action=end";

                        request.Add("amount", amount.ToString());
                        request.Add("buyOrder", buyOrder.ToString());
                        request.Add("sessionId", sessionId.ToString());
                        request.Add("urlReturn", urlReturn.ToString());
                        request.Add("urlFinal", urlFinal.ToString());

                        /** Ejecutamos metodo initTransaction desde Libreria */
                        wsInitTransactionOutput result = webpay.getNormalTransaction().initTransaction(amount, buyOrder, sessionId, urlReturn, urlFinal);

                        /** Verificamos respuesta de inicio en webpay */
                        if (result.token != null && result.token != "")
                        {
                            message = "Sesion iniciada con exito en Webpay";
                        }
                        else
                        {
                            message = "webpay no disponible";
                        }

                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(result) + "</p>");

                        HttpContext.Current.Response.Write("" + message + "</br></br>");
                        HttpContext.Current.Response.Write("<form action=" + result.url + " method='post'><input type='hidden' name='token_ws' value=" + result.token + "><input type='submit' value='Continuar &raquo;'></form>");

                    }
                    catch (Exception ex)
                    {
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br> Ocurri&oacute; un error en la transacci&oacute;n (Validar correcta configuraci&oacute;n de parametros). " + ex.Message + "</p>");
                    }

                    break;

                case "result":

                    tx_step = "Get Result";

                    try
                    {

                        HttpContext.Current.Response.Write("<p style='font-weight: bold; font-size: 150%;'>Step: " + tx_step + "</p>");

                        /** Obtiene Información POST */
                        string[] keysPost = Request.Form.AllKeys;

                        /** Token de la transacción */
                        string token = Request.Form["token_ws"];

                        request.Add("token", token.ToString());

                        transactionResultOutput result = webpay.getNormalTransaction().getTransactionResult(token);

                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br> " + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br> " + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(result) + "</p>");

                        if (result.detailOutput[0].responseCode == 0)
                        {
                            message = "Pago ACEPTADO por webpay (se deben guardatos para mostrar voucher)";

                            HttpContext.Current.Response.Write("<script>localStorage.setItem('authorizationCode', " + result.detailOutput[0].authorizationCode + ")</script>");
                            HttpContext.Current.Response.Write("<script>localStorage.setItem('commercecode', " + result.detailOutput[0].commerceCode + ")</script>");
                            HttpContext.Current.Response.Write("<script>localStorage.setItem('amount', " + result.detailOutput[0].amount + ")</script>");
                            HttpContext.Current.Response.Write("<script>localStorage.setItem('buyOrder', " + result.detailOutput[0].buyOrder + ")</script>");

                        }
                        else
                        {
                            message = "Pago RECHAZADO por webpay [Codigo]=> " + result.detailOutput[0].responseCode + " [Descripcion]=> " + codes[result.detailOutput[0].responseCode.ToString()];
                        }

                        HttpContext.Current.Response.Write(message + "</br></br>");
                        HttpContext.Current.Response.Write("<form action=" + result.urlRedirection + " method='post'><input type='hidden' name='token_ws' value=" + token + "><input type='submit' value='Continuar &raquo;'></form>");
                    }
                    catch (Exception ex)
                    {
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br> Ocurri&oacute; un error en la transacci&oacute;n (Validar correcta configuraci&oacute;n de parametros). " + ex.Message + "</p>");
                    }

                    break;

                case "end":

                    tx_step = "End";

                    try
                    {

                        HttpContext.Current.Response.Write("<p style='font-weight: bold; font-size: 150%;'>Step: " + tx_step + "</p>");

                        request.Add("", "");

                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(Request.Form["token_ws"]) + "</p>");

                        message = "Transacci&oacute;n Finalizada";
                        HttpContext.Current.Response.Write(message + "</br></br>");

                        string next_page = sample_baseurl + "?action=capture";

                        HttpContext.Current.Response.Write("<form action=" + next_page + " method='post'><input type='hidden' name='commercecode' id='commercecode' value=''><input type='hidden' name='authorizationCode' id='authorizationCode' value=''><input type='hidden' name='amount' id='amount' value=''><input type='hidden' name='buyOrder' id='buyOrder' value=''><input type='submit' value='Capturar Transacci&oacute;n &raquo;'></form>");
                        HttpContext.Current.Response.Write("<script>var commercecode = localStorage.getItem('commercecode');document.getElementById('commercecode').value = commercecode;</script>");
                        HttpContext.Current.Response.Write("<script>var authorizationCode = localStorage.getItem('authorizationCode');document.getElementById('authorizationCode').value = authorizationCode;</script>");
                        HttpContext.Current.Response.Write("<script>var amount = localStorage.getItem('amount');document.getElementById('amount').value = amount;</script>");
                        HttpContext.Current.Response.Write("<script>var buyOrder = localStorage.getItem('buyOrder');document.getElementById('buyOrder').value = buyOrder;</script>");

                    }
                    catch (Exception ex)
                    {
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br> Ocurri&oacute; un error en la transacci&oacute;n (Validar correcta configuraci&oacute;n de parametros). " + ex.Message + "</p>");
                    }

                    break;

                case "capture":

                    tx_step = "capture";

                    try
                    {

                        HttpContext.Current.Response.Write("<p style='font-weight: bold; font-size: 150%;'>Step: " + tx_step + "</p>");

                        /** Obtiene Información POST */
                        string[] keysNullify = Request.Form.AllKeys;

                        /** Código de autorización de la transacción que se requiere anular */
                        string authorizationCode = Request.Form["authorizationCode"];

                        /** Monto que se desea capturar de la transacción */
                        decimal captureAmount = 200;

                        /** Orden de compra de la transacción que se requiere anular */
                        buyOrder = Request.Form["buyOrder"];

                        request.Add("authorizationCode", authorizationCode.ToString());
                        request.Add("captureAmount", captureAmount.ToString());
                        request.Add("buyOrder", buyOrder.ToString());

                        captureOutput result = webpay.getCaptureTransaction().capture(authorizationCode, captureAmount, buyOrder);

                        request.Add("", "");

                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(result) + "</p>");

                        message = "Transacci&oacute;n Finalizada";
                        HttpContext.Current.Response.Write(message + "</br></br>");

                    }
                    catch (Exception ex)
                    {
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightyellow;'><strong>request</strong></br></br>" + new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(request) + "</p>");
                        HttpContext.Current.Response.Write("<p style='font-size: 100%; background-color:lightgrey;'><strong>result</strong></br></br> Ocurri&oacute; un error en la transacci&oacute;n (Validar correcta configuraci&oacute;n de parametros). " + ex.Message + "</p>");
                    }

                    break;
            }

            HttpContext.Current.Response.Write("</br><a href='default.aspx'>&laquo; volver a index</a>");

        }
    }
}