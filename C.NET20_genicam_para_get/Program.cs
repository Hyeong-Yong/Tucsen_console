using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TUCAMAPI;

namespace C.NET20_genicam_para_get
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
                        Console.WriteLine("Get element attribute list:");
                        printElementInfo();
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


        #region print all element infomation
        static void printElementInfo()
        {
            try
            {
                m_node.pName = Marshal.StringToHGlobalAnsi(string.Format("Root"));
            int level = 0;
            string[] access = new string[]{"NI","NA", "WO", "RO","RW" };

            string[] elemType = new string[]{"Value",
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

            while (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_GenICam_ElementAttrNext(m_opCam.hIdxTUCam, ref m_node, m_node.pName))
            {

                switch (m_node.Type)
                {
                    case TUELEM_TYPE.TU_ElemCategory:
                        {
                            //level = Math.Max(level, (byte)0);
                            level = m_node.Level;
                            Console.WriteLine();
                            Console.WriteLine(String.Format("{0} [{1}] [{2}]", level, m_node.Level, Marshal.PtrToStringAnsi(m_node.pName)));

                            break;
                        }
                    case TUELEM_TYPE.TU_ElemInteger:
                        {
                            string printStr = string.Format("{0} [{1}] [{2}][{3}] {4}, {5}", level, m_node.Level, access[(int)m_node.Access], elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t1.nVal);
                            Console.WriteLine(printStr);
                            break;
                        }
                    case TUELEM_TYPE.TU_ElemBoolean:
                        {

                            string printStr = string.Format("{0} [{1}] [{2}][{3}] {4}, {5}", level, m_node.Level, access[(int)m_node.Access], elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t1.nVal);
                            Console.WriteLine(printStr);
                            break;
                        }
                    case TUELEM_TYPE.TU_ElemCommand:
                        {
                            string printStr = string.Format("{0} [{1}] [{2}][{3}] {4}, {5}", level, m_node.Level, access[(int)m_node.Access], elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t1.nVal);
                            Console.WriteLine(printStr);
                            break;
                        }
                    case TUELEM_TYPE.TU_ElemFloat:
                        {

                            string printStr = string.Format("{0} [{1}] [{2}][{3}] {4}, {5}", level, m_node.Level, access[(int)m_node.Access], elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), m_node.uValue.t2.dbVal);
                            Console.WriteLine(printStr);
                            break;
                        }
                    case TUELEM_TYPE.TU_ElemString:
                        {
                            IntPtr pStr = IntPtr.Zero;
                            pStr = Marshal.AllocHGlobal((int)m_node.uValue.t1.nMax + 1);
                            m_node.pTransfer = pStr;

                            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_GenICam_ElementAttr(m_opCam.hIdxTUCam, ref m_node, m_node.pName))
                            {
                                string printStr = string.Format("{0} [{1}] [{2}][{3}] {4}, {5}", level, m_node.Level, access[(int)m_node.Access], elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName), Marshal.PtrToStringAnsi(m_node.pTransfer));
                                Console.WriteLine(printStr);
                            }
                            break;
                        }
                    case TUELEM_TYPE.TU_ElemRegister:
                        {
                            string printStr = string.Format("{0} [{1}] [{2}][{3}] {4}", level, m_node.Level, access[(int)m_node.Access], elemType[(int)m_node.Type], Marshal.PtrToStringAnsi(m_node.pName));
                            Console.WriteLine(printStr);
                            break;
                        }
                    case TUELEM_TYPE.TU_ElemEnumeration:
                        {

                            string printStr = string.Format("{0} [{1}] [{2}][{3}] {4}, {5}", 
                                                                level, 
                                                                m_node.Level, 
                                                                access[(int)m_node.Access], 
                                                                elemType[(int)m_node.Type], 
                                                                Marshal.PtrToStringAnsi(m_node.pName), 
                                                                Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(m_node.pEntries, (int) m_node.uValue.t1.nVal * 8)));

                            Console.WriteLine(printStr);

                            //all enum
                            //for (int i = 0; i <= m_node.uValue.t1.nMax; i++)
                            //{
                            //    string enumStr = string.Format("  val:{0}, enum value:{1} \r\n", i, Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(m_node.pEntries, i * 8)));//8 byte
                            //    Console.Write(enumStr);
                            //}
                            break;
                        }
                    default:
                        break;
                } //switch end!

            }//while end!
            }
            catch (Exception)
            {
                Console.WriteLine("Get element attribute list finish");
                throw;
            }
        }
        #endregion
    }
}
