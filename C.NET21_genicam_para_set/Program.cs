using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TUCAMAPI;


namespace C.NET21_genicam_para_set
{
    class Program
    {
        public static TUCAM_INIT m_itApi;          // SDK API initialized object
        public static TUCAM_OPEN m_opCam;          // Open camera object
        public static TUCAM_ELEMENT m_node;

        static void Main(string[] args)
        {
            try
            {
                if (TUCAMRET.TUCAMRET_SUCCESS == InitApi())
                {
                    m_opCam.uiIdxOpen = 0;
                    if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0))
                    {
                        Console.WriteLine("Open Camera success");

                        //Set String\Enumeration\Float\Interger\Bool 
                        float exposureTimes = 5.0f;
                        int analogGain = 1;
                        int blackLevel = 100;
                        int reverseX = 1;
                        string curDeviceUserID = "Dhyana 2100";

                        //set string or register
                        setPropertyValue<string>(TUELEM_TYPE.TU_ElemString, curDeviceUserID, m_elements[0]);

                        //Enumeration
                        setPropertyValue<int>(TUELEM_TYPE.TU_ElemEnumeration, analogGain, m_elements[1]);

                        //Float
                        setPropertyValue<float>(TUELEM_TYPE.TU_ElemFloat, exposureTimes, m_elements[2]);

                        //Interger
                        setPropertyValue<int>(TUELEM_TYPE.TU_ElemInteger, blackLevel, m_elements[3]);

                        //Bool
                        setPropertyValue<int>(TUELEM_TYPE.TU_ElemBoolean, reverseX, m_elements[4]);


                        Console.WriteLine("\r\n press any key to exit");
                        Console.ReadKey();
                        CloseCamera();

                    }
                    else
                    {
                        Console.WriteLine("Open Camera fail!");
                    }

                    UnInitApi();
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
                Console.WriteLine();
            }
            finally
            {
                Console.Write("press any key exit!");
                Console.ReadKey();
            }

        }

        #region init 、uninit
        /* Init the TUCAM API */
        static TUCAMRET InitApi()
        {
            /* Get the current directory */
            IntPtr strPath = Marshal.StringToHGlobalAnsi(System.Environment.CurrentDirectory);

            m_itApi.uiCamCount = 0;
            m_itApi.pstrConfigPath = strPath;

            TUCamAPI.TUCAM_Api_Init(ref m_itApi);

            Console.WriteLine("Connect {0} camera", m_itApi.uiCamCount);

            if (0 == m_itApi.uiCamCount)
            {
                return TUCAMRET.TUCAMRET_NO_CAMERA;
            }

            return TUCAMRET.TUCAMRET_SUCCESS;
        }


        /* UnInit the TUCAM API */
        static TUCAMRET UnInitApi()
        {
            return TUCamAPI.TUCAM_Api_Uninit();
        }


        /* Open the camera by index number */
        static TUCAMRET OpenCamera(uint uiIdx)
        {
            if (uiIdx >= m_itApi.uiCamCount)
            {
                return TUCAMRET.TUCAMRET_OUT_OF_RANGE;
            }

            m_opCam.uiIdxOpen = uiIdx;

            return TUCamAPI.TUCAM_Dev_Open(ref m_opCam);
        }


        /* Close the current camera */
        static TUCAMRET CloseCamera()
        {
            if (null != m_opCam.hIdxTUCam)
            {
                TUCamAPI.TUCAM_Dev_Close(m_opCam.hIdxTUCam);
            }

            Console.WriteLine("Close the camera success");

            return TUCAMRET.TUCAMRET_SUCCESS;
        }

        #endregion


        #region set element value

        public static string[] m_elements = { "DeviceUserID", "AnalogGain", "ExposureTime", "BlackLevel", "ReverseX" };//ExposureTime
        public static string[] elemType = new string[]{"Value",
                                            "Base",
                                            "Integer",
                                            "Boolean",
                                            "Command",
                                            "Float",
                                            "String",
                                            "Register",
                                            "Category",
                                            "Enumeration",
                                            "EnumEntry",
                                            "Port"};
        static void SetElement()
        {

            double exposureTimes = 5.0;
            int analogGain = 1;
            int blackLevel = 100;
            int reverseX = 1;
            IntPtr curDeviceUserID = Marshal.StringToHGlobalAnsi("Dhyana 2100");


            //set TU_TU_ElemString
            m_node.pName = Marshal.StringToHGlobalAnsi(m_elements[0]);
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_GenICam_GetElementValue(m_opCam.hIdxTUCam, ref m_node))
            {
                m_node.pTransfer = curDeviceUserID;
                TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);
                Console.WriteLine(string.Format("[{1}] Set {1} value is {2}", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), Marshal.PtrToStringAnsi(m_node.pTransfer)));

                IntPtr pStr = IntPtr.Zero;
                pStr = Marshal.AllocHGlobal((int)m_node.uValue.t1.nMax + 1);
                m_node.pTransfer = pStr;
                TUCamAPI.TUCAM_GenICam_GetElementValue(m_opCam.hIdxTUCam, ref m_node);

                Console.WriteLine(string.Format("[{1}] Get {1} value is {2}", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), Marshal.PtrToStringAnsi(m_node.pTransfer)));
            }


            //set TU_ElemEnumeration
            m_node.pName = Marshal.StringToHGlobalAnsi(m_elements[1]);
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_GenICam_GetElementValue(m_opCam.hIdxTUCam, ref m_node))
            {
                m_node.uValue.t1.nVal = Math.Max(0, analogGain);
                m_node.uValue.t1.nVal = Math.Min(m_node.uValue.t1.nVal, m_node.uValue.t1.nMax);
                TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);

                Console.WriteLine(string.Format("[{0} Set {1} value is {2}]", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(m_node.pEntries, (int)m_node.uValue.t1.nVal * 8))));
            }


            //set TU_ElemFloat
            m_node.pName = Marshal.StringToHGlobalAnsi(m_elements[2]);
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_GenICam_GetElementValue(m_opCam.hIdxTUCam, ref m_node))
            {
                m_node.uValue.t2.dbVal = exposureTimes;
                TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);

                Console.WriteLine(string.Format("[{0} Set {1} value is {2}]", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t2.dbVal));
            }


            //set TU_ElemInterger
            m_node.pName = Marshal.StringToHGlobalAnsi(m_elements[3]);
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_GenICam_GetElementValue(m_opCam.hIdxTUCam, ref m_node))
            {
                m_node.uValue.t1.nVal = blackLevel;
                TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);

                Console.WriteLine(string.Format("[{0} Set {1} value is {2}]", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t1.nVal));
            }


            //set TU_ElemBoolean
            m_node.pName = Marshal.StringToHGlobalAnsi(m_elements[4]);
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_GenICam_GetElementValue(m_opCam.hIdxTUCam, ref m_node))
            {
                m_node.uValue.t1.nVal = reverseX;
                TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);

                Console.WriteLine(string.Format("[{0} Set {1} value is {2}]", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t1.nVal));
            }


        }

        static void setPropertyValue<T>(TUELEM_TYPE type, T param, string name)
        {
            try
            {
                m_node.pName = Marshal.StringToHGlobalAnsi(name);

                switch (type)
                {
                    case TUELEM_TYPE.TU_ElemInteger:
                        {
                            m_node.uValue.t1.nVal = Convert.ToInt64(param);
                            TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);

                            Console.WriteLine(string.Format("[{0}] Set {1} value is {2}", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t1.nVal));
                        }
                        break;
                    case TUELEM_TYPE.TU_ElemBoolean:
                        {
                            m_node.uValue.t1.nVal = Convert.ToInt64(param);
                            TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);

                            Console.WriteLine(string.Format("[{0}] Set {1} value is {2}", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t1.nVal));
                        }
                        break;
                    case TUELEM_TYPE.TU_ElemCommand:
                        {
                            m_node.uValue.t1.nVal = Convert.ToInt64(param);
                            TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);

                            Console.WriteLine(string.Format("[{0}] Set {1} value is {2}", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t1.nVal));
                        }
                        break;
                    case TUELEM_TYPE.TU_ElemFloat:
                        {
                            m_node.uValue.t2.dbVal = Convert.ToDouble(param);
                            TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);

                            Console.WriteLine(string.Format("[{0}] Set {1} value is {2}", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t2.dbVal));
                        }
                        break;
                    case TUELEM_TYPE.TU_ElemString:
                        {
                            m_node.pTransfer = Marshal.StringToHGlobalAnsi(Convert.ToString(param));
                            TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);
                            Console.WriteLine(string.Format("[{0}] Set {1} value is {2}", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), Marshal.PtrToStringAnsi(m_node.pTransfer)));

                            IntPtr pStr = IntPtr.Zero;
                            pStr = Marshal.AllocHGlobal((int)m_node.uValue.t1.nMax + 1);
                            m_node.pTransfer = pStr;
                            TUCamAPI.TUCAM_GenICam_GetElementValue(m_opCam.hIdxTUCam, ref m_node);

                            Console.WriteLine(string.Format("[{0}] Get {1} value is {2}", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), Marshal.PtrToStringAnsi(m_node.pTransfer)));
                        }
                        break;
                    case TUELEM_TYPE.TU_ElemRegister:
                        break;
                    case TUELEM_TYPE.TU_ElemEnumeration:
                        {
                            TUCamAPI.TUCAM_GenICam_ElementAttr(m_opCam.hIdxTUCam, ref m_node, m_node.pName);

                            m_node.uValue.t1.nVal = Math.Max(0, Convert.ToInt32(param));
                            m_node.uValue.t1.nVal = Math.Min(m_node.uValue.t1.nVal, m_node.uValue.t1.nMax);
                            TUCamAPI.TUCAM_GenICam_SetElementValue(m_opCam.hIdxTUCam, ref m_node);

                            Console.WriteLine(string.Format("[{0}] Set {1} value is {2}", elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(m_node.pEntries, (int)m_node.uValue.t1.nVal * 8))));
                        }
                        break;
                    default:
                        break;
                }

            }
            catch (Exception)
            {
                Console.WriteLine("set property value fail"); ;
                throw;
            }

        }



        #endregion
    }
}
