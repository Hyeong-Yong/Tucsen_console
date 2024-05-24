using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TUCAMAPI;

namespace C.NET14_histogram
{
    class Program
    {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_OPEN m_opCam;       // Open camera object
        public static TUCAM_FRAME m_frame;      // The frame object
        public static TUCAM_CAPA_ATTR m_capaAttr;

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

        static void Main(string[] args)
        {

            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi())
            {
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0))
                {
                    Console.WriteLine("Open the camera success");
                    GetHistogramData();
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

        static void GetHistogramData()
        {
            m_frame.pBuffer = IntPtr.Zero;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            m_frame.uiRsdSize = 1;

            TUCamAPI.TUCAM_Buf_Alloc(m_opCam.hIdxTUCam, ref m_frame);
            Console.WriteLine("Set histogram enable");
            TUCamAPI.TUCAM_Capa_SetValue(m_opCam.hIdxTUCam, (int)TUCAM_IDCAPA.TUIDC_HISTC, 1);

            m_capaAttr.idCapa = (int)TUCAM_IDCAPA.TUIDC_HISTC;
            m_capaAttr.nValMin = 0;
            m_capaAttr.nValMax = 0;
            m_capaAttr.nValDft = 0;
            m_capaAttr.nValStep = 0;
            TUCamAPI.TUCAM_Capa_GetAttr(m_opCam.hIdxTUCam, ref m_capaAttr);

            int histogramValue = 0;

            TUCamAPI.TUCAM_Capa_GetValue(m_opCam.hIdxTUCam, (int)TUCAM_IDCAPA.TUIDC_HISTC, ref histogramValue);
            Console.WriteLine("histogram value:{0}", histogramValue);


            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(m_opCam.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_SEQUENCE))
            {


                if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_WaitForFrame(m_opCam.hIdxTUCam, ref m_frame, 1000))
                {
                    Console.WriteLine("Grab the frame success, width:{0}, height:{1}, channel:{2}, depth:{3}, image size:{4}",  m_frame.usWidth, m_frame.usHeight, m_frame.ucChannels, (2 == m_frame.ucElemBytes) ? 16 : 8, m_frame.uiImgSize);

                    UInt32 offset = m_frame.uiImgSize + m_frame.usHeader;
                    UInt32 hislen = 65536 * 4; //int32 65536
                    UInt32 chnn = 4;
                    Int32  hisChannel = 0;    //color 0:Y 1:R 2:G 3:B
                    
                    int hisSize = (int)hislen;
                    if (1 == m_frame.ucElemBytes)
                        hisSize = 256 * 4;

                    //Console.WriteLine("channel :{0}", m_frame.ucChannels);
                    byte[] his = new byte[hisSize];
                    if (m_frame.ucChannels == 1) //mono 0:Y
                    {
                        Marshal.Copy(m_frame.pBuffer, his, (int)offset, hisSize);
                    }
                    else
                    {
                        Console.WriteLine("Get channel = {0} histogram offset:{1}, hisSize:{2}", hisChannel, offset, hisSize);

                        TUCamAPI.TUCAM_Capa_SetValue(m_opCam.hIdxTUCam, (Int32)TUCAM_IDCAPA.TUIDC_CHANNELS, hisChannel);
                        //Marshal.Copy(m_frame.pBuffer, his, (int)(offset + hisChannel * hislen), hisSize);
                        for (int i = 0; i < hisSize; i++)
                        {
                           uint value = (uint)Marshal.ReadInt32(m_frame.pBuffer,i + (int)offset);
                           if (value != 0)
                                Console.WriteLine("i:{0}, value:{1}", i, value);
                        }
                    }                
                }
                else
                {
                    Console.WriteLine("Grab the frame failure");
                }
                

                TUCamAPI.TUCAM_Buf_AbortWait(m_opCam.hIdxTUCam);
                TUCamAPI.TUCAM_Cap_Stop(m_opCam.hIdxTUCam);
            }

            TUCamAPI.TUCAM_Buf_Release(m_opCam.hIdxTUCam);

        }
    }
}
