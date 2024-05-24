using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using TUCAMAPI;

namespace C.NET12_trigger_output
{
    class Program
    {
        public static TUCAM_INIT m_itApi;          // SDK API initialized object
        public static TUCAM_OPEN m_opCam;          // Open camera object
        public static TUCAM_TRGOUT_ATTR m_tgrout;  // The output trigger object

        public static string[] m_strPort = { "TRIG.OUT1", 
                                             "TRIG.OUT2", 
                                             "TRIG.OUT3"
                                            };

        public static string[] m_strKind = { "Low", 
                                             "High", 
                                             "IN", 
						                     "Exposure Start", 
						                     "Global Exposure", 
						                     "Read End", 
                                           };

        public static string[] m_strEdge = { "Rising", 
                                             "Failing",  
                                           };

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

        /* Do OutPut trigger */
        static void DoOutputTrigger()
        {
	        if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_GetTriggerOut(m_opCam.hIdxTUCam, ref m_tgrout))
	        {
		        Console.WriteLine("Get output trigger success, Port:%s, Kind:%s, Edge:%s, Delay time:%d, Width:%d", m_strPort[m_tgrout.nTgrOutPort], m_strKind[m_tgrout.nTgrOutMode], m_strEdge[m_tgrout.nEdgeMode], m_tgrout.nDelayTm, m_tgrout.nWidth);
	        }
	        else
	        {
		        Console.WriteLine("Get output trigger failure");
	        }

	        if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_SetTriggerOut(m_opCam.hIdxTUCam, m_tgrout))
	        {
		        Console.WriteLine("Set output trigger success, Port:%s, Kind:%s, Edge:%s, Delay time:%d, Width:%d", m_strPort[m_tgrout.nTgrOutPort], m_strKind[m_tgrout.nTgrOutMode], m_strEdge[m_tgrout.nEdgeMode], m_tgrout.nDelayTm, m_tgrout.nWidth);
	        }
	        else
	        {
		        Console.WriteLine("Set output trigger failure");
	        }
        }

        static void Main(string[] args)
        {
                        
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi())
            {
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0))
                {
                    Console.WriteLine("Open the camera success");

                    Console.WriteLine();
                    DoOutputTrigger();
                    Console.WriteLine();

                    CloseCamera();
                }
                else
                {
                    Console.WriteLine("Open the camera failure");
                }

                UnInitApi();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
