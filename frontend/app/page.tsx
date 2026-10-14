// "use client"

// import {useEffect, useState} from "react";


// export default function Home() {

//   const [status, setStatus]= useState("...checking");

//   useEffect(() => {
//     fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/health`)
//     .then((res) => res.json())
//     .then((data) => setStatus(data.status))
//     .catch(()=> setStatus("API unreachable"));
//   })

//   return (
//     <div className="flex min-h-screen items-center justify-center">
//       <div className = "text-center">
//          <h1 className="text-3xl font-bold">Support Desk</h1>
//         <p className="mt-2">API status: {status}</p>
//       </div>
//     </div>
//   );
// }


import { redirect } from "next/navigation";

export default function Home() {
  redirect("/tickets");
}