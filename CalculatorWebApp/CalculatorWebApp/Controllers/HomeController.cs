using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class HomeController : Controller
{
    private static string m_strHistory = "";
    private static string m_strResult = "0";
    private static decimal? m_decResult = 0;
    private static bool m_bolIsZeroPress = false;
    private static string m_strLastOperation = "";
    private static bool m_bolIsPressed = false;
    private static decimal m_decOldNum = 0;
    private static string strNotDivideByZero = "Can not divide by zero.";
    private static string strOutOfRange = "Result is out of range.";
    private static bool boolIsException = false;

    char[] charsToTrimFromResult = { '+', '-', '*', ' ', '/' };

    public IActionResult Index()
    {
        ViewBag.DisplayHistory = m_strHistory;
        ViewBag.DisplayResult = m_strResult;
        return View();
    }

    [HttpPost]
    public IActionResult HandleNumber([FromBody] ValueRequest data)
    {

        // check any exception is occur or not
        if (boolIsException == true)
        {
            ViewData["IsButtonEnabled"] = false;
            return View();
        }

        // check if result length = 9 and result is not contains dot
        if (m_strResult.Length == 9 && !m_strResult.Contains("."))
        {
            return View();
        }

        // check if history and result both are null or zero
        if (m_strResult == "0")
        {
            m_bolIsZeroPress = false;
        }

        // check if equal button is not press
        if (m_bolIsPressed)
        {
            HandleClearAll();
        }

        // get number from this method's parameter
        string strNumber = data.Value;

        // check entered number is zero or not and also check result is zero
        if (strNumber == "0" && m_strResult == "0")
        {
            m_bolIsZeroPress = true;
        }
        int maxDigits = m_strResult.Contains(".") ? 10 : 9;

        // it is not contains dot in 9 digit
        if (m_strResult.Length >= maxDigits)
        {
            return View();
        }

        if (strNumber == ".")
        {

            // this is prevent multiple dot
            if (m_strResult.Contains("."))
            {
                return View();
            }

            // if not containt any dot then add dot
            m_strResult += strNumber;
        }

        // this code for normally perform
        else
        {

            // this code is remove 0 from first character
            if (m_strResult == "0")
            {

                // enter any number then this is replace zero with entered number
                m_strResult = strNumber;
            }
            else
            {

                // this code is used for add more than one number
                m_strResult += strNumber;
            }
        }

        // this code for if not press any operation sign..
        if (m_strLastOperation == "")
        {
            return Json(new { displayResult = m_strResult });
        }
        else
        {
            return Json(new { displayHistory = m_strHistory, displayResult = m_strResult });
        }
    }

    [HttpPost]
    public IActionResult HandleOperation([FromBody] ValueRequest data)
    {

        // this code for any exception occur then not press any operation sign
        if (boolIsException == true)
        {
            ViewData["IsButtonEnabled"] = false;
            return View();
        }

        // this variable for press any operation sign and store that in this variable
        string strOperation = data.Value;

        if (m_bolIsPressed && m_strHistory.EndsWith("="))
        {
            m_strLastOperation = strOperation;
            m_strHistory = "";
            m_strHistory = $"{m_strResult}{strOperation}";
            m_strResult = "0";
            m_bolIsPressed = false;
        }

        // this code for when press any operation sign without any number then use
        if (m_strResult == "0")
        {

            // this code for when change operation sign then prevent history value make 0
            if (charsToTrimFromResult.Contains(m_strHistory.LastOrDefault(c => charsToTrimFromResult.Contains(c))))
            {
                m_strHistory = m_strHistory.Substring(0, m_strHistory.Length - 1);
            }
            else
            {
                m_strHistory = "0";
            }
        }

        // this is convert string value to decimal
        decimal decNum = decimal.Parse(m_strResult);

        if (!string.IsNullOrEmpty(m_strLastOperation))
        {
            // for continuous operation (10 + 2 + 4 + 234 + ...)
            PerformOperation(m_strLastOperation, decNum);
            if (boolIsException == true)
            {

                // 10 / 0 then press operation sign (+)
                ViewData["IsButtonEnabled"] = false;
                return Json(new { displayHistory = $"{m_strHistory}/{decNum}=", displayResult = strNotDivideByZero });
            }
        }
        else
        {

            //this is for press operation sign without enter any number (direct press -(minus))
            m_decResult = decimal.Parse(m_strResult);
        }
        if (m_strHistory.EndsWith("="))
        {

            // this is for when press = button
            m_bolIsPressed = false;
        }

        // this is for if any operation sign press or not
        if (decNum != 0)
        {

            // it will add and append in history
            m_strHistory = $"{m_strHistory}{decNum}{strOperation}";
        }
        else if (decNum.ToString().StartsWith("0.00"))
        {
            m_strHistory = $"0{strOperation}";
            m_strResult = "0";
        }
        else
        {

            // if entered number is not enter and press any sign and direct enter any operation sign
            m_strHistory = $"{m_strHistory}{strOperation}";
        }

        m_strResult = "0";

        // reinitialize variable
        m_strLastOperation = strOperation;

        ViewBag.DisplayHistory = m_strHistory;
        ViewBag.DisplayResult = m_strResult;

        // for this calculation :- 999999998+1+1
        if (FormatResult(m_decResult) == strOutOfRange)
        {
            boolIsException = true;
            ViewData["IsButtonEnabled"] = false;
            return Json(new { displayHistory = $"{m_strHistory.TrimEnd(charsToTrimFromResult)}", displayResult = strOutOfRange });
        }

        // this is for check history not greater then 40
        string strHisto = FormatHistory(m_strHistory);
        return Json(new { displayHistory = strHisto, displayResult = FormatResult(m_decResult) });
    }

    [HttpPost]
    public IActionResult HandleEqual()
    {

        // this code for check any exception is not occur
        if (boolIsException == true)
        {
            ViewData["IsButtonEnabled"] = false;
            return View();
        }

        if (m_strLastOperation == "/" && m_strResult == "0" && m_bolIsZeroPress == true)
        {

            // this code for if last operation is / and result(means second value) is 0
            boolIsException = true;
            ViewData["IsButtonEnabled"] = false;
            return Json(new { displayHistory = $"{m_strHistory}{m_strResult}=", displayResult = strNotDivideByZero });
        }

        if (m_strResult.EndsWith("."))
        {

            // it is remove dot at last character when press equal button(2. + 3. + 4. => 2 + 3 + 4) if this code is not then it will work like this(2. + 3. + 4. => 2 + 3 + 4.)
            // this is for when second value enter 4. then press equal then prevent 4. to 
            m_strResult = m_strResult.Substring(0, m_strResult.Length - 1);
        }
        m_bolIsPressed = true;

        if (!string.IsNullOrEmpty(m_strLastOperation))
        {

            // this line for trim result screen if enter 10 + then work 10
            decimal decResultNum = decimal.Parse(m_strResult);

            // this code for if second value is not enter
            if (m_strResult == "0")
            {
                if (m_decResult == 0 && m_strLastOperation == "-" && m_bolIsZeroPress == false)
                {
                    return HandleCalculation(m_strLastOperation, m_decOldNum, decResultNum);
                }

                // use for trim operation sign(5 - =)=> history is :- -5--5= result is 10
                m_strHistory = m_strHistory.Trim(charsToTrimFromResult);

                // this code for is press zero or not
                if (m_bolIsZeroPress)
                {
                    decResultNum = 0;
                }
                else if(!m_strHistory.Contains(m_strHistory.LastOrDefault(c => charsToTrimFromResult.Contains(c))) && !m_strHistory.Contains("="))
                {

                    // this code for when 0 is not press and consider first value as second value 
                    decResultNum = decimal.Parse(m_strHistory);
                }
                if (m_strLastOperation != null)
                {
                    if (!m_strHistory.EndsWith("="))
                    {

                        // this vdecOldNum variable contains old value 10 + = then 10 is old value
                        m_decOldNum = decResultNum;
                    }
                    return HandleCalculation(m_strLastOperation, m_decOldNum, (decimal)m_decResult);
                }
                
                return Json(new { displayHistory = m_strHistory + "=", displayResult = FormatResult(m_decResult) });
                
            }

            // this code for if second value is enter
            else
            {
                if (m_strLastOperation != null)
                {
                    if (!m_strHistory.EndsWith("="))
                    {
                        // 10 + 2 = then 2 is old value for continuous press = button operation
                        m_decOldNum = decResultNum;
                    }
                    return HandleCalculation(m_strLastOperation, m_decOldNum, m_decResult.Value);
                }
            }
        }

        ViewBag.DisplayHistory = m_strHistory;
        ViewBag.DisplayResult = m_strResult;

        return Json(new { displayHistory = FormatHistory(m_strHistory), displayResult = FormatResult(decimal.Parse(m_strResult)) });
    }

    private IActionResult HandleCalculation(string operation, decimal decResultNum, decimal decInitialValue)
    {
        // this method for calculation
        PerformOperation(operation, decResultNum);

        // for display any exception message
        if (FormatResult(m_decResult) == strOutOfRange)
        {
            boolIsException = true;
            ViewData["IsButtonEnabled"] = false;
            return Json(new { displayHistory = $"{decInitialValue}{m_strLastOperation}{decResultNum}=", displayResult = strOutOfRange });
        }
        else if (m_strResult == strNotDivideByZero)
        {
            boolIsException = true;
            ViewData["IsButtonEnabled"] = false;
            return Json(new { displayHistory = $"{m_strHistory.Trim(charsToTrimFromResult)}{m_strLastOperation}{decResultNum}=", displayResult = strNotDivideByZero });
        }
        else
        {
            m_strHistory = $"{FormatResult(decInitialValue)}{operation}{FormatResult(decResultNum)}=";
            m_strResult = FormatResult(m_decResult);

            ViewBag.DisplayHistory = m_strHistory;
            ViewBag.DisplayResult = m_strResult;

            return Json(new { displayHistory = FormatHistory(m_strHistory), displayResult = FormatResult(m_decResult) });
        }
    }

    // common calculation
    private void PerformOperation(string operation, decimal num)
    {
        decimal decN = num;
        // this condition for when change sign
        if ((!m_bolIsZeroPress && m_strLastOperation == "*" && !m_bolIsPressed) || (!m_bolIsZeroPress && m_strLastOperation == "/" && !m_bolIsPressed))
        {
            // purpose of num = 1 is when change operation sign / to remaining than not make exception
            num = 1;
        }

        if (charsToTrimFromResult.Contains(m_strHistory.LastOrDefault(c => charsToTrimFromResult.Contains(c))))
        {
            // purpose of num = decN is when perform like this operation for example :- 25/2 then press +
            num = decN;
        }
        switch (operation)
        {
            case "+":
                m_decResult += num;
                break;
            case "-":
                m_decResult -= num;
                break;
            case "*":
                m_decResult *= num;
                break;
            case "/":
                if (num != 0)
                {
                    m_decResult /= num;
                }
                else
                {
                    ViewBag.ErrorMessage = strNotDivideByZero;
                    m_strResult = strNotDivideByZero;
                    boolIsException = true;
                    ViewData["IsButtonEnabled"] = false;
                    return;
                }
                break;
        }

        m_strResult = FormatResult(m_decResult);
    }

    public IActionResult HandleClearAll()
    {
        // clear all only when press delete and c or C button
        m_strHistory = "";
        m_strResult = "0";
        m_decResult = 0;
        m_decOldNum = 0;
        m_strLastOperation = "";
        m_bolIsPressed = false;
        m_bolIsZeroPress = false;
        boolIsException = false;
        return Json(new { displayHistory = m_strHistory, displayResult = m_strResult });
    }

    [HttpPost]
    public IActionResult HandleClearLast()
    {

        // clear last value
        string strAHistory = m_strHistory;
        string strBResult = m_strResult;

        if (strBResult == "0" && charsToTrimFromResult.Contains(m_strHistory.LastOrDefault(c => charsToTrimFromResult.Contains(c))))
        {
            return View();
        }

        // this is for is exception occur?
        if (boolIsException == true)
        {
            ViewData["IsButtonEnabled"] = false;
            return View();
        }

        // remove last character
        if (!string.IsNullOrEmpty(strBResult) && strBResult.Length >= 1)
        {
            if (m_strHistory.EndsWith("="))
            {

                // in history at the last contains = button then not remove character
                return View();
            }

            // this code remove last character from result              
            m_strResult = m_strResult.Remove(strBResult.Length - 1);
            if (m_strResult == "" && !m_strResult.EndsWith(charsToTrimFromResult.ToString()))
            {

                // this code for if result is null then make zero in result
                m_strResult = "0";
            }
        }

        ViewBag.DisplayResult = m_strResult;

        return Json(new { displayHistory = strAHistory, displayResult = m_strResult });
    }

    private string FormatResult(decimal? result)
    {

        // this is for format result and return exception and (1.5356  => 1.54)
        if (result == null)
        {
            return "";
        }

        // this variable for store result value which is pass in parameter
        string strResultString = result.Value.ToString();

        if (strResultString.Contains('.'))
        {
            strResultString = Math.Round(result.Value, 2).ToString("0.##");

            // Check if result string is greater than 10 characters
            if (strResultString.Length > 10)
            {
                strResultString = strOutOfRange;
            }
        }
        else
        {

            // if result string contains - than minus not contain in 9 digit
            if (strResultString.Contains("-") && strResultString.Length > 9 && strResultString.Length < 11)
            {
                return strResultString;
            }

            // if result string is greater than 9 than result is out of range
            if (strResultString.Length > 9)
            {
                strResultString = strOutOfRange;
            }
        }
        return strResultString;
    }

    private string FormatHistory(string? history)
    {

        // for handle history 40 character
        // if history null than return
        string strHistoryString = history;

        // it will remove full string when history is greater than 40 
        // 111111111+222222222+111111111+333333333 now add any one digit for example + 1 then first 111111111 + is remove and history will be 222222222+111111111+333333333+1
        if (strHistoryString.Length > 40)
        {
            int index = m_strHistory.IndexOfAny(charsToTrimFromResult);

            strHistoryString = strHistoryString.Substring(index);
            m_strHistory = strHistoryString;

        }

        return strHistoryString;
    }

    public class ValueRequest
    {

        // this is for get value from user and pass to controller
        public string Value { get; set; }
    }
}